@ECHO OFF

REM Step 1 - Generate the reflection information
"{@SandcastlePath}ProductionTools\MRefBuilder" /config:MRefBuilder.config /out:reflection.org {@Dependencies} {@DocInternals} *.dll *.exe

IF ERRORLEVEL 1 GOTO Exit

REM Merge duplicate topics (if any)
COPY /Y reflection.org reflection.all

"{@SandcastlePath}ProductionTools\XslTransform" /xsl:"{@SandcastlePath}ProductionTransforms\MergeDuplicates.xsl" reflection.all /out:reflection.org

:Exit
