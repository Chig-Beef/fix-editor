# fix-editor
Keyboard based editor.

I wanted to find an editor that I could become fast with, but instead of being smart and just using neovim I thought it would be a better idea to just make my own?
The editor is written in C#.
It's quite basic so far, here are the current features:

1. Basic text editing (you'd hope)
2. A file explorer (also keyboard based)
3. A config file so users can choose their configuration
4. Colours for certain file extensions (configurable)
5. Size of files are displayed in file explorer (configurable)
6. Run and build programs (running is usually fine, building still needs working out)

Here are the current commands
1. "fe" => Opens file explorer
2. "nf" <fileName> => Creates a new file (in the currently opened directory).
3. "nd" <dirName> => Creates a new directory.
4. "q" => Quits.
5. "mw" => Enter write mode (^q takes you back to command mode).
6. "me" => Enter explore mode.
7. "fs" => Saves the current file.
8. "fsa" => Saves all currently open files.
9. "s" => Takes you back to the start screen.
10. "lf" => Lists all currently open files (this comes up at the bottom of the editor and yes it is coloured by file extensions).
11. "gf" <fileName | index> => Goto a currently open file with the specified name, or use the index (which can be easy to find by using "lf").
12. "rf" <fileName | index> => Removes the specified file.
13. "help" => Brings up the help page.
14. "cr" => Runs the config code again (done on startup, but can be used after chaning your config to get the changes).
15. "run" => Runs the currently open source file.
16. "build" => Builds the currently open source file.
17. "br" => Builds and runs the currently open source file.
