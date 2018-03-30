REM # For VS8(mine):
set VCDIR=c:\dev\visualstudio8\vc
set SDKDIR=C:\dev\winsdk
set VCINCL_ADD=/I%SDKDIR%\include /I%SDKDIR%\include\crt /D_CRT_SECURE_NO_DEPRECATE
set VCLIB_ADD=/LIBPATH:%SDKDIR%\lib /LIBPATH:%VCDIR%\lib
set PATH=%VCDIR%\bin;%SDKDIR%\bin;%VCDIR%\..\common7\ide;%VCDIR%\..\common7\tools\bin;%PATH%

set INCLUDE=%SDKDIR%\include;%SDKDIR%\include\crt;%VCDIR%\include
set LIB=%SDKDIR%\lib;%VCDIR%\lib

nmake -f ipmi_sample.mak all
