@ECHO OFF

REM Step 4.2 - Build the reference help topics
"{@SandcastlePath}ProductionTools\BuildAssembler" /config:sandcastle.config manifest.xml
