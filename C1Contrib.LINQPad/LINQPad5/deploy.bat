xcopy /i/y/s header.xml %1\header.xml
del C1Contrib.LINQPad.lpx

"C:\Program Files\7-Zip\7z.exe" a C1Contrib.LINQPad.zip %1\*.*

ren C1Contrib.LINQPad.zip C1Contrib.LINQPad.lpx