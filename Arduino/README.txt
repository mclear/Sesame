This is a basic proof of concept example for the Adafruit PN532 shield on an Arduino Uno

The required Adafruit library can be found at : https://github.com/adafruit/Adafruit-PN532

There is a very basic serial protocol. Commands are all in upper-case and are delimited by newline '\n' characters.

On connect, send "SHAKE" from the PC and expect a response of "NFC Reader". Otherwise I assume this is some other device and skip.
Once I have determined there is an arduino nfc reader, I send "IDENT" and expect back a string that contains the reader name (hard-coded in this example).
Sending "START" enables the NFC read loop. Sending "STOP" disables it.
Scanning an NFC tag sends a hex string of the ID to the serial port, followed by a newline '\n' (char 10);

If you create new commands, I've limited the size of the buffer to 8, so either keep it to less than 8 characters, or increase the size of the buffer.