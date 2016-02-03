# Object Mapper
This is simple library with interfaces for mapping one C# object to another with code analyser feature of .NET compiler platform (aka Roslyn) to provide code generation.

## Why another mapper
All other mappers operate in runtime using some kind of reflection to generate code which maps properties between objects. While this removes the need to write boilerplate code and speeds up development, it has number of disadvantages such as:
- Refactoring unfriendly - if someone renames one property but not the other, mapping does not work on that property anymore
- No way to see what will actually get copied - code is generated at the runtime
- Runtime code generation and reflection has performance overhead

This is why this library consists of simple interfaces for object mapping contract only. Provided code analyser and code fix provider are responsible for generating mapping boilerplate code for developer. Generated code can be modified if desired, you actually see what gets mapped and there is no performance overhead since mapping code is like any other hand written code.

## Usage

Install NuGet package to your project - you can find current release [here](https://www.nuget.org/packages/SimpleObjectMapper/). This will add ObjectMapper.Framework to your references and diagnostic analyser under analyzers node.
You can then use mapper interfaces from ObjectMapper.Framework assembly to define mapping. For example let's define two classes with some matching properties:

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

Let's say that we want to define mapping from class ClassA to ClassB. We specify that ClassA implements IObjectMapper<ClassB> interface. If we have cursor on IObjectMapper<ClassB> symbol, Visual Studio will display lightbulb on the right side. By clicking on lightbulb (or using space + . shortcut), you will get menu of options. If you click 'Generate implementation' interface implementation with mapping code will be generated for you. For the above example result is as follows:

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

If you do not want to have mapping code inside your POCO classes you can use IObjectMapperAdapter<T,U>. By having cursor on IObjectMapperAdapter symbol, you can invoke 'Generate implementation' and generator will generate mapping code for mapping from class T to class U and from class U to class T.

