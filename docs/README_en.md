Very Lasy Wave Player
1. [Overview]
It is a simple audio player that can play while displaying the waveform of a WAVE file (.wav).

2. [Features]
It corresponds to his WAVE file in the format below.
Unsigned 8bit PCM
Signed 16bit PCM (little endian)
Signed 32bit PCM (little endian)
Signed 64bit PCM (little endian) 1
IEEE float 32bit PCM (little endian)
IEEE float 64bit PCM (little endian) 1
You can find the precise start position of a specific phrase by operating the mouse or keyboard. The position accuracy is approximately 10 ms.
3. [Required environment]
.NET Framework 4.8
4. [Assumed use]
It is assumed that you want to obtain a highly accurate position of a specific phrase.

A typical example is when you want to adjust the timing when editing the ".lrc" file.

5. [How to operate]
5.1 How to Start
Run "WavePlayer.exe".

5.2 How to open an audio file
Select the audio file to open from "Open" in the "File" menu.

Alternatively, after associating "WavePlayer.exe" with a file with a ".wav" extension, you can open it by clicking the ".wav" file from Explorer.

5.3 Keyboard operation
The following shortcut keys are supported.

5.3.1 Play/Stop Control
Action when key is pressed
Playback starts or stops as the space marker advances.
ESC Playback stops.
S Step playback is performed. The marker will play for 10ms and then stop.
ENTER Plays without moving the marker from the marker position.
5.3.2 Move marker position
Action when key is pressed
The right cursor marker advances 100 milliseconds.
Ctrl+Right Cursor Marker advances 1 second.
The up cursor marker advances 10 seconds.
Ctrl+Up Cursor Marker advances 60 seconds.
The left cursor marker moves back 100 milliseconds.
Ctrl+Left Cursor Moves the marker back one second.
The down cursor marker moves back 10 seconds.
Ctrl+Down Cursor Marker moves back 60 seconds.
The HOME marker returns to its initial position.
5.3.3 Clipboard related
Action when key is pressed
Ctrl+C Copies the current marker position to the clipboard. (Example: String "[01:53.22]" for 1 minute 53.22 seconds)
Ctrl+V Moves the marker to the position represented by the time text (eg "[01:53.22]") on the clipboard.
5.3.4 Change volume
Action when key is pressed
+ Increases volume by one step.
- Decrease volume by one step.
5.3.5 Zoom in/out waveform
Action when key is pressed
PageUp Waveform expands by one step.
PageDown The waveform shrinks by one step.
5.4 Mouse operation
Operation Run-time behavior
Left-click the waveform screen Moves the marker to the clicked position.
Left-click the upper timeline display The marker moves to the clicked position.
Ctrl+mouse wheel rotation Expands/contracts the waveform step by step.
6. [Notice]
Please note that the developer will not be held responsible for any damage caused by using this software.
The only audio files supported by this application are WAVE files (extension ".wav").
7. [Appendix]
7.1 About waveform display
This application only displays one waveform regardless of the number of channels in the audio file.

This is because the main purpose of this application is to find the precise position of a specific phrase on an audio file, and the waveform display is to help you see changes in volume.

Therefore, only one overall volume change for all channels of the audio file is displayed as a waveform.

For the same reason, the sampling accuracy of the waveform display is about 10 milliseconds, which is sufficiently accurate for practical use, and waveform changes shorter than that are not displayed accurately.

The waveform display is normalized based on the peak volume. As a result, volume changes are clearly visible even for audio files with overall low volume.

Footnotes
Waveforms can be displayed, but no sound is played. This seems to be due to a limitation of .NET Framework 4.8.
