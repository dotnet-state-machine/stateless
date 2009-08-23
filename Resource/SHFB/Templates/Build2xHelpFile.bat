@ECHO OFF

REM Step 7.2 - Build the HTML 2.x help file
cd .\Output
copy "..\{@HTMLHelpName}.HxT" . > NUL
copy ..\Help2x.HxC "{@HTMLHelpName}.HxC" > NUL
copy ..\Help2x.HxF "{@HTMLHelpName}.HxF" > NUL
copy ..\Help2x_A.HxK "{@HTMLHelpName}_A.HxK" > NUL
copy ..\Help2x_B.HxK "{@HTMLHelpName}_B.HxK" > NUL
copy ..\Help2x_F.HxK "{@HTMLHelpName}_F.HxK" > NUL
copy ..\Help2x_K.HxK "{@HTMLHelpName}_K.HxK" > NUL
copy ..\Help2x_NamedURLIndex.HxK "{@HTMLHelpName}_NamedURLIndex.HxK" > NUL
copy ..\Help2x_S.HxK "{@HTMLHelpName}_S.HxK" > NUL
IF EXIST ..\StopWordList.txt copy ..\StopWordList.txt StopWordList.txt > NUL

"{@HXCompPath}hxcomp" -p "{@HTMLHelpName}.HxC" -l "{@HTMLHelpName}.log"

type "{@HTMLHelpName}.log"

cd ..

IF EXIST "{@OutputFolder}{@HTMLHelpName}.Hx?" DEL "{@OutputFolder}{@HTMLHelpName}.Hx?" > NUL
IF EXIST "{@OutputFolder}{@HTMLHelpName}_?.Hx?" DEL "{@OutputFolder}{@HTMLHelpName}_?.Hx?" > NUL
copy ".\Output\*.Hx?" "{@OutputFolder}" > NUL
del "{@OutputFolder}\{@HTMLHelpName}.HxF" > NUL

REM Must remove these in case we are building a 1x file or website as well
IF EXIST ".\Output\{@HTMLHelpName}.log" del ".\Output\{@HTMLHelpName}.log" > NUL
IF EXIST .\Output\StopWordList.txt del .\Output\StopWordList.txt > NUL
del .\Output\*.Hx? > NUL
