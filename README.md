# Object Mapper
This is simple library with interfaces for mapping one C# object to another with code analyser feature of .NET compiler platform (aka Roslyn) to provide code generation.

## Why another mapper
All other mappers operate in runtime using some kind of reflection to generate code which maps properties between objects. While this removes the need to write boilerplate code and speeds up development, it has number of disadvantages such as:
- Refactoring unfriendly - if someone renames one property but not the other, mapping does not work on that property anymore
- No way to see what will actually get copied - code is generated at the runtime
- Runtime code generation and reflection has performance overhead

This is why this library consists of simple interfaces for object mapping contract only. Provided code analyser and code fix provider are responsible for generating mapping boilerplate code for developer. Generated code can be modified if desired, you actually see what gets mapped and there is no performance overhead since mapping code is like any other hand written code.

## Usage


