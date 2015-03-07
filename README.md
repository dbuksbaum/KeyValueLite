# KeyValueLite

Simple Key Value Data Store Over SQLite
---------------------------------------

I needed a simple Key Value data store to use with SQLite, and didn't want
to repeat the logic and management of the underlying data store in my projects,
so I created this simple library to deal with it for me.

To use the library, make sure you have a copy of the sqlite3.dll in the 
application folder or path. This build is for the 32bit SQLite only.

This project uses [Fody.Scalpel](https://github.com/Fody/Scalpel) to remove 
the unit tests and testing dependencies in Release mode.

Uses my fork of [sqlite-net](https://github.com/dbuksbaum/sqlite-net).

Version 1.0 - 2015-03-xx
------------------------
  * Initial code drop
  * Tested with SQLite 3.8.8.3


