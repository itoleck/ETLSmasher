# ETLSmasher

## Compress, remove providers and remove events after a timeframe.

## Usage

- Compress and remove events with provider ed54dff8-c409-4cf6-bf83-05e1e61a09c4(WinSATAssessment) and remove events after 60 seconds in the trace.

ETLSmasher.exe --infile:"z:\temp\cpufreq.etl" --outfile:"z:\temp\cpufreq_compressed.etl" --compress:true --removeproviders:ed54dff8-c409-4cf6-bf83-05e1e61a09c4 --removetimeafter:60000

- Compress and remove events with provider ed54dff8-c409-4cf6-bf83-05e1e61a09c4(WinSATAssessment)
  
ETLSmasher.exe --infile:"z:\temp\cpufreq.etl" --outfile:"z:\temp\cpufreq_compressed.etl" --compress:true --removeproviders:ed54dff8-c409-4cf6-bf83-05e1e61a09c4

- Remove events after 60 seconds in the trace, do not compress.

ETLSmasher.exe --infile:"z:\temp\cpufreq.etl" --outfile:"z:\temp\cpufreq_compressed.etl" --removetimeafter:60000

- Only compress a trace.

ETLSmasher.exe --infile:"z:\temp\cpufreq.etl" --outfile:"z:\temp\cpufreq_compressed.etl" --compress:true

## To find the ETW event provider guids in a trace use WPA.exe.

![Alt text](ETLSmasher.png)
