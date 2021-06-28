# Nuget Package

> [Latest](https://www.nuget.org/packages/LazuplisMei.BinarySerializer/)
> - **add BigIntegerConverter**
> - **add ComplexConverter**
> - **add VectorConverter**
> - **public ListConverter**
> - **add Utilities.ReadDouble**
> - **add Utilities.WriteDouble**
> - **add Utilities.ReadFloat**
> - **add Utilities.WriteFloat**

> [1.0.3](https://www.nuget.org/packages/LazuplisMei.BinarySerializer/1.0.3)
> - **add BinaryConverterAttribute**

> [1.0.2](https://www.nuget.org/packages/LazuplisMei.BinarySerializer/1.0.2)
> - **add Serializer.SerializeWithTypeInfo**
> - **add Serializer.DeserializeWithTypeInfo**

> [1.0.1.1](https://www.nuget.org/packages/LazuplisMei.BinarySerializer/1.0.1.1)
> - **add BinaryIndexAttribute**

> [1.0.0.1](https://www.nuget.org/packages/LazuplisMei.BinarySerializer/1.0.0.1)
> - **internl LazuplisMei.BinarySerializer.Converter**
> - ***void* IBinarySerializable.Deserialize(use IBinarySerializable object itself)**
> - **add xml comment**


> [1.0.0](https://www.nuget.org/packages/LazuplisMei.BinarySerializer/1.0.0)


# Built-In-Converters
- ## ElementTypeConverter
  - ### `bool`
  - ### `char`
  - ### `short`
  - ### `int`
  - ### `long`
  - ### `ushort`
  - ### `uint`
  - ### `ulong`
  - ### `float`
  - ### `double`
  - ### `byte`
  - ### `sbyte`
  - ### `decimal`
- ## NullableConverter
  - ### `Nullable<T>`
- ## ListConverter
  - ### `List<T>`
- ## IEnumerableConverter
  - ### Any`IEnumerable<T>` as `List<T>`
- ## ArrayConverter
  - ### Any Rank `Array`
- ## HashSetConverter
  - ### `HashSet<T>`
- ## StackConverter
  - ### `Stack<T>`
- ## QueueConverter
  - ### `Queue<T>`
- ## DictionaryConverter
  - ### `Dictionary<TKey,TValue>`
- ## KeyValuePairConverter
  - ### `KeyValuePair<TKey,TValue>`
- ## StringConverter
  - ### `string`
- ## DateTimeConverter
  - ### `DateTime`
- ## DateTimeOffsetConverter
  - ### `DateTimeOffset`
- ## TimeSpanConverter
  - ### `TimeSpan`
- ## EnocdingConverter
  - ### `Enocding`
- ## TypeConverter
  - ### `Type`
- ## BigIntegerConverter
  - ### `BigInteger`
- ## ComplexConverter
  - ### `Complex`
- ## VectorConverter
  - ### `Vector<T>`
- ## ObjectConverter
  - ### `Quaternion`
  - ### `Matrix3x2`
  - ### `Matrix4x4`
  - ### `Vector2`
  - ### `Vector3`
  - ### `Vector4`
  - ### `Plane`
  - ### Any `Enum`
  - ### other general types with accessible properties

# Simple Example

```cs
class Student
{
    public string Name;
    public int Id;
}

byte[] buffer = Serializer.Serialize(new Student() {Name = "Name001", Id = 1000 });
Student stu = Serializer.Deserialize<Student>(buffer);
//stu.Name == "Name001" && stu.Id == 1000
```

# BinaryIgnoreAttribute

```cs
class Student
{
    public string Name;
    [BinaryIgnore]
    public int Id;
}

byte[] buffer = Serializer.Serialize(new Student() {Name = "Name001", Id = 1000 });
Student stu = Serializer.Deserialize<Student>(buffer);
//stu.Name == "Name001" && stu.Id == default
```

# BinaryIndexAttribute

```cs
class TestClass
{
    [BinaryIndex(1)]
    public string AAA;
    [BinaryIndex(0)]
    public int BBB;
}

//order by index then by name, BBB will be the first field to serialize
byte[] buffer = Serializer.Serialize(new TestClass() {AAA = "AAA", BBB = 0 });
TestClass stu = Serializer.Deserialize<TestClass>(buffer);
```

# BinaryConverterAttribute

```cs
class Student
{
    //this field with use MyStringConverter to (de)serialize
    //but Serializer.SerializeWithTypeInfo and Serializer.DeserializeWithTypeInfo will ignore this attribute and use default internal StringConverter
    [BinaryConverter(typeof(MyStringConverter))]
    public string Name;
    public int Id;
}

byte[] buffer = Serializer.Serialize(new Student() {Name = "Name001", Id = 1000 });
Student stu = Serializer.Deserialize<Student>(buffer);
//stu.Name based on MyStringConverter's behavior && stu.Id == 1000
```

# add assembly to find type

```cs
class Student
{
    public string Name;
    public int Id;
}

byte[] buffer = Serializer.Serialize(typeof(Student));
Type stuType = Serializer.Deserialize<Type>(buffer);
//TypeNotFoundException

Serializer.AddAssembly(Assembly.GetExecutingAssembly());//AddAssembly
byte[] buffer = Serializer.Serialize(typeof(Student));
Type stuType = Serializer.Deserialize<Type>(buffer);
```

# SerializeWithTypeInfo

```cs
class Student
{
    public string Name;
    public int Id;
}

var stu = new Student() {Name = "Name001", Id = 1000 };
var memory = new MemoryStream();
Serializer.SerializeWithTypeInfo(stu, memory);
memory.Seek(0, SeekOrigin.Begin);
//result is a ExpaandoObject
dynamic result = Serializer.DeserializeWithTypeInfo(memory);
//result.Name == "Name001" && result.Id == 1000
```



# Private Member

```cs
class Student
{
    public string Name;
    private int Id;
    public Student()
    {
        //default ctor is required
    }
    public Student(string name,int id)
    {
        Name = name;
        Id = id;
    }
}

byte[] buffer = Serializer.Serialize(new Student("Name001", 1000));
Student stu = Serializer.Deserialize<Student>(buffer);
//stu.Name == Name001 && stu.Id == default

Serializer.OnlyPublicMember = false;
byte[] buffer = Serializer.Serialize(new Student("Name001", 1000));
Student stu = Serializer.Deserialize<Student>(buffer);
//stu.Name == Name001 && stu.Id == 1000
```
# Custom BinaryConverter

```cs

//EnocdingConverter already exists
class EnocdingConverter : BinaryConverter<Encoding>
{
    public override Encoding ReadBytes(Stream stream)
    {
        return Encoding.GetEncoding(stream.ReadInt32());
    }
    public override void WriteBytes(Encoding obj, Stream stream)
    {
        stream.WriteInt32(obj.CodePage);
    }
}

//AddConverter
Serializer.AddConverter<EnocdingConverter>();
```

# BinaryConstructorAttribute

```cs
class Student
{
    public string Name;
    public int Id;
    //you can add default value
    [BinaryConstructor("name")]
    //this ctor can be private
    public Student(string name, int id)
    {
        Name = name;
        Id = id;
    }
    public Student()
    {
        //if this default ctor exists
        //[BinaryConstructor] is meaningless
        //or it's required
    }
}
```

# IBinarySerializable

```cs
class Person : IBinarySerializable
{
    public string Name { get; private set; }

    [BinaryConstructor]//required
    public Person(string name)
    {
        Name = name;
    }

    public void Serialize(Stream stream)
    {
        Serializer.Serialize(Name, stream);
    }

    public void Deserialize(Stream stream)
    {
        Name = Serializer.Deserialize<string>(stream);
    }
}
```

# Custom GenericConverter

```cs
//HashSetConverter already exists
[GenericConverter(typeof(HashSet<>))]
class HashSetConverter : GenericConverter
{
    public override object GenericReadBytes(Stream stream)
    {
        object hashset = Activator.CreateInstance(CurrentType);
        var hashset_Add = CurrentType.GetMethod("Add");

        int count = stream.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            hashset_Add.Invoke(hashset, Serializer.Deserialize(TypeArgs[0], stream));
        }
        return hashset;
    }

    public override void GenericWriteBytes(Stream stream, object obj)
    {
        //ListConverter is internal, here just for example
        ListConverter.ListWriteBytes(TypeArgs[0], stream, obj);
    }
}

//AddConverter
Serializer.AddConverter<HashSetConverter>();
```

# ObjectConverter

```cs
//Whether to use it depends on the situation
//ObjectConverter does not have basic size check and null check
//Use it for external information that only has a certain data structure
class DataInfo
{
    //use BinaryIndex ensure that the data is read in the correct order
    [BinaryIndex(0)]
    public int BBB;
    [BinaryIndex(1)]
    public int AAA; 
}
var data = ObjectConverter.ReadBytes<DataInfo>(File.OpenRead(@"C:\xxx.bin"));
```