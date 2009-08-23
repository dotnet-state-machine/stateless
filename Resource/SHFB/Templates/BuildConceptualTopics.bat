@ECHO OFF

REM Step 4.1 - Build the reference help topics
"{@SandcastlePath}ProductionTools\BuildAssembler" /config:conceptual.config ConceptualManifest.xml
