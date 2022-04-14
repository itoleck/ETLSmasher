Imports System
Imports Microsoft.Diagnostics.Tracing

Module Program

    Dim infile As String = ""
    Dim outfile As String = ""
    Dim compress As Boolean = False
    Dim providers(-1) As String
    Dim relogtime As Int64 = -1
    Dim totalevents As Int64 = 0

    Sub Main()
        Main(Environment.GetCommandLineArgs())
    End Sub

    Private Sub Main(ByVal args() As String)
        If args.Length > 1 Then
            Select Case args(1).ToLower
                Case "help"
                    ShowHelp()
                    End
                Case "-help"
                    ShowHelp()
                    End
                Case "--help"
                    ShowHelp()
                    End
                Case "/help"
                    ShowHelp()
                    End
                Case "h"
                    ShowHelp()
                    End
                Case "-h"
                    ShowHelp()
                    End
                Case "--h"
                    ShowHelp()
                    End
                Case "/h"
                    ShowHelp()
                    End
                Case "?"
                    ShowHelp()
                    End
                Case "-?"
                    ShowHelp()
                    End
                Case "--?"
                    ShowHelp()
                    End
                Case "/?"
                    ShowHelp()
                    End
            End Select

            Console.ForegroundColor = ConsoleColor.Red

            For Each arg In args
                If arg.StartsWith("--infile:") Then
                    infile = arg.Replace("'", "").Replace("""", "").Trim(" ").Remove(0, 9)
                End If
            Next

            For Each arg In args
                If arg.StartsWith("--outfile:") Then
                    outfile = arg.Replace("'", "").Replace("""", "").Trim(" ").Remove(0, 10)
                End If
            Next

            For Each arg In args
                If arg.StartsWith("--removeproviders:") Then
                    providers = arg.Replace("'", "").Replace("""", "").Trim(" ").Remove(0, 18).ToLower.Split(",")
                End If
            Next

            For Each arg In args
                If arg.StartsWith("--removetimeafter:") Then
                    Try
                        relogtime = CType(arg.Replace("'", "").Replace("""", "").Trim(" ").Remove(0, 18), Int64)
                    Catch ex As Exception
                        Console.WriteLine("Can't change " + arg + " to a millisecond")
                        ShowHelp()
                    End Try
                End If
            Next

            For Each arg In args
                If arg.StartsWith("--compress:") Then
                    Dim tf As String
                    tf = arg.Replace("'", "").Replace("""", "").Trim(" ").Remove(0, 11).ToLower
                    If tf = "true" Or tf = "t" Or tf = "1" Then
                        compress = True
                    Else
                        compress = False
                    End If
                End If
            Next

            If relogtime = -1 And providers.Length = 0 Then
                RelogByCompressOnly()
            Else
                If providers.Length > 0 Then
                    Relog()
                End If
            End If

        Else
            ShowHelp()
        End If

        Console.ResetColor()
        Console.WriteLine("Saved: " + totalevents.ToString + " events")

    End Sub

    Private Sub RelogByCompressOnly()

        Console.WriteLine("Compressing trace: " + infile)

        Using relogger As New ETWReloggerTraceEventSource(infile, TraceEventSourceType.FileOnly, outfile)
            relogger.OutputUsesCompressedFormat = compress

            'Keeping around so I remember the other evetns
            'AddHandler relogger.Dynamic.All, Function(ByVal data As Microsoft.Diagnostics.Tracing.TraceEvent)
            '                                     relogger.WriteEvent(data)
            '                                 End Function

            'AddHandler relogger.Kernel.All, Function(ByVal data As Microsoft.Diagnostics.Tracing.TraceEvent)
            '                                    relogger.WriteEvent(data)
            '                                End Function

            'AddHandler relogger.Clr.All, Function(ByVal data As Microsoft.Diagnostics.Tracing.TraceEvent)
            '                                 relogger.WriteEvent(data)
            '                             End Function

            AddHandler relogger.AllEvents, Function(ByVal data As Microsoft.Diagnostics.Tracing.TraceEvent)
                                               relogger.WriteEvent(data)
                                               totalevents += 1
                                           End Function

            relogger.Process()
        End Using
    End Sub

    Private Sub Relog()

        Console.WriteLine("Re-Logging trace: " + infile)

        Using relogger As New ETWReloggerTraceEventSource(infile, TraceEventSourceType.FileOnly, outfile)
            relogger.OutputUsesCompressedFormat = compress

            AddHandler relogger.AllEvents, Function(ByVal data As Microsoft.Diagnostics.Tracing.TraceEvent)

                                               If relogtime = -1 Then

                                                   Dim uid As String = data.TaskGuid.ToString.ToLower
                                                   Dim idx As Int32 = Array.IndexOf(providers, uid)

                                                   If idx = -1 Then
                                                       relogger.WriteEvent(data)
                                                       totalevents += 1
                                                   Else
                                                       Debug.WriteLine(data.TaskGuid.ToString)
                                                   End If

                                               Else

                                                   'Some events may  be written at trace rundown, might be good to keep last few seconds. More inveestigation needed.
                                                   'If (CType(data.TimeStampRelativeMSec, Int32) < relogtime) Or (data.TimeStampRelativeMSec > (data.Source.SessionEndTimeRelativeMSec - 10000))

                                                   If (CType(data.TimeStampRelativeMSec, Int32) < relogtime) Then

                                                       Dim uid As String = data.TaskGuid.ToString.ToLower
                                                       Dim idx As Int32 = Array.IndexOf(providers, uid)

                                                       If idx = -1 Then
                                                           relogger.WriteEvent(data)
                                                           totalevents += 1
                                                       Else
                                                           Debug.WriteLine(data.TaskGuid.ToString)
                                                       End If

                                                   Else
                                                       Debug.WriteLine(data.ProviderName)
                                                   End If

                                               End If

                                           End Function

            relogger.Process()
        End Using
    End Sub

    Private Sub ShowHelp()
        Console.ResetColor()
        Console.WriteLine("")
        Console.WriteLine("Program will open and read a .etl trace and produce a smaller .etl file based on the options selected.")
        Console.WriteLine("")
        Console.WriteLine("Accepted arguments:")
        Console.WriteLine("h | -h | --h | /h | help | -help | --help | /help | ? | -? | --? | /? Shows this help screen")
        Console.WriteLine("")
        Console.WriteLine("--infile:<ETLFILENAME>" + vbTab + "REQUIRED")
        Console.WriteLine("--outfile:<ETLFILENAME>" + vbTab + "REQUIRED")
        Console.WriteLine("--compress:true or t or 1")
        Console.WriteLine("--removeproviders:<guid>,<guid>,<guid>")
        Console.WriteLine("--removetimeafter:<milliseconds to start removal of events>")
        Console.WriteLine("")
        Console.WriteLine("Examples:")
        Console.WriteLine("ETLSmasher.exe --infile:c:\trace.etl --outfile:c:\trace_compressed.etl --compress:true")
        Console.WriteLine("ETLSmasher.exe --infile:c:\trace.etl --outfile:c:\trace_compressed.etl --removeproviders:e13c0d23-ccbc-4e12-931b-d9cc2eee27e4,a669021c-c450-4609-a035-5af59af4df18,57277741-3638-4a4b-bdba-0ac6e45da56c")
        Console.WriteLine("ETLSmasher.exe --infile:c:\trace.etl --outfile:c:\trace_compressed.etl --compress:true --removeproviders:e13c0d23-ccbc-4e12-931b-d9cc2eee27e4,a669021c-c450-4609-a035-5af59af4df18,57277741-3638-4a4b-bdba-0ac6e45da56c")
        Console.WriteLine("ETLSmasher.exe --infile:c:\trace.etl --outfile:c:\trace_compressed.etl --compress:true --removeproviders:e13c0d23-ccbc-4e12-931b-d9cc2eee27e4,a669021c-c450-4609-a035-5af59af4df18,57277741-3638-4a4b-bdba-0ac6e45da56c --removetimeafter:60000")
        Console.WriteLine("")
        Console.WriteLine("")
        End
    End Sub

End Module
