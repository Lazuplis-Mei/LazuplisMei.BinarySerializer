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