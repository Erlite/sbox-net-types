# sbox-net-types
A collection of network type wrappers for things like System.Collections.Generic.

# How to use

Simple, replace the following types with my wrappers:

- List\<T\> => NetworkedList\<T\>
- Dictionary\<T\> => NetworkedDirectory\<T\>
- NetworkVar\<T\> will automatically call NetworkDirty() and raise an event on change.

# License

Check LICENSE.MD, but basically, do whatever you want with it.
@facepunch if you wanna add this, go ahead, my pleasure.
