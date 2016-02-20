# Object Mapper
This is simple library with interfaces for mapping one C# object to another with code analyser feature of .NET compiler platform (aka Roslyn) to provide code generation.

## Why another mapper
All other mappers operate in runtime using some kind of reflection to generate code which maps properties between objects. While this removes the need to write boilerplate code and speeds up development, it has number of disadvantages such as:
- Refactoring unfriendly - if someone renames one property but not the other, mapping does not work on that property anymore
- No way to see what will actually get copied - code is generated at the runtime
- Runtime code generation and reflection has performance overhead

This is why this library consists of simple interfaces for object mapping contract only. Provided code analyser and code fix provider are responsible for generating mapping boilerplate code for developer. Generated code can be modified if desired, you actually see what gets mapped and there is no performance overhead since mapping code is like any other hand written code.

## Usage

Install NuGet package to your project - you can find current release [here](https://www.nuget.org/packages/SimpleObjectMapper/). This will add ObjectMapper.Framework to your references and diagnostic analyser under analyzers node. You can then use mapper interfaces or attribute from ObjectMapper.Framework assembly to define mapping. 

### IObjectMapper<T> interface

Let's define two classes with some matching properties:

```C#
using ObjectMapper.Framework;
using System.Collections.Generic;

namespace TestClassLibrary
{
    public class ClassA
    {
        public int? Prop1 { get; set; }
        public string Prop2 { get; set; }
        public decimal Prop3 { get; set; }

        private List<int> _prop4;
        public List<int> Prop4 { get { if (_prop4 == null) _prop4 = new List<int>(); return _prop4; } }
    }

    public class ClassB
    {
        public int Prop1 { get; set; }
        public string Prop2 { get; set; }
        public decimal Prop3 { get; set; }

        private List<int> _prop4;
        public List<int> Prop4 { get { if (_prop4 == null) _prop4 = new List<int>(); return _prop4; } }
    }
}
```

Let's say that we want to define mapping from class ClassA to ClassB. We specify that ClassA implements IObjectMapper<ClassB> interface. If we have caret on IObjectMapper<ClassB> symbol, Visual Studio will display lightbulb on the right side. By clicking on lightbulb (or using left control + . shortcut), you will get menu of options. If you click 'Generate implementation' interface implementation with mapping code will be generated for you. For the above example result is as follows:

```C#
using ObjectMapper.Framework;
using System.Collections.Generic;

namespace TestClassLibrary
{
    public class ClassA : IObjectMapper<ClassB>
    {
        public int? Prop1 { get; set; }
        public string Prop2 { get; set; }
        public decimal Prop3 { get; set; }

        private List<int> _prop4;
        public List<int> Prop4 { get { if (_prop4 == null) _prop4 = new List<int>(); return _prop4; } }

        public void MapObject(ClassB target)
        {
            target.Prop1 = this.Prop1 ?? default(int);
            target.Prop2 = this.Prop2;
            target.Prop3 = this.Prop3;
            target.Prop4.CopyFrom(this.Prop4);
        }
    }

    public class ClassB
    {
        public int Prop1 { get; set; }
        public string Prop2 { get; set; }
        public decimal Prop3 { get; set; }

        private List<int> _prop4;
        public List<int> Prop4 { get { if (_prop4 == null) _prop4 = new List<int>(); return _prop4; } }
    }
}
```

If you have method already defined (or generated), you can set caret on method name and 'Generate implementation' action will be available.

### IObjectMapperAdapter<T, U> interface

If you do not want to have mapping code inside your POCO classes you can use IObjectMapperAdapter<T,U>. By having cursor on IObjectMapperAdapter symbol, you can invoke 'Generate implementation' and generator will generate mapping code for mapping from class T to class U and from class U to class T.

If we take previous example of ClassA and ClassB and we define new class Adapter which implements IObjectMapperAdapter<ClassA, ClassB> generated code for Adapter class is as follows:

```C#
public class Adapter : IObjectMapperAdapter<ClassA, ClassB>
{
    public void MapObject(ClassA source, ClassB target)
    {
        target.Prop1 = source.Prop1 ?? default(int);
        target.Prop2 = source.Prop2;
        target.Prop3 = source.Prop3;
        target.Prop4.CopyFrom(source.Prop4);
    }

    public void MapObject(ClassB source, ClassA target)
    {
        target.Prop1 = source.Prop1;
        target.Prop2 = source.Prop2;
        target.Prop3 = source.Prop3;
        target.Prop4.CopyFrom(source.Prop4);
    }
}
```

### ObjectMapperMethodAttribute attribute

If you want to have ad hoc method for mapping without actually implementing one of the above interfaces, you can annotate mapping method with ObjectMapperMethodAttribute attribute. This attribute can be applied only to methods with following signatures:
- Method accepts exactly two parameters
- Method return type is void

Compile time error will be raised otherwise. If you then place caret on method name, lightbulb will apear and 'Generate implementation' action will be available.

Example of method with applied attribute:

```C#
[ObjectMapperMethod]
private static void MapFromAToB(ClassA source, ClassB target)
{
    target.Prop1 = source.Prop1 ?? default(int);
    target.Prop2 = source.Prop2;
    target.Prop3 = source.Prop3;
    target.Prop4.CopyFrom(source.Prop4);
}
```

## Code generation rules

1. Property names must match in both classes.
2. Only public properties are considered. Also source getter and target setter must be public.
3. Type must match exactly. Currently there is no check if source object/value is assignable to target.
4. If source and target types are collections, mapping code will copy objects from one collection to another if generic type is the same. Non generic collections are not supported.

