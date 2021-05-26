using LazuplisMei.BinarySerializer.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LazuplisMei.BinarySerializer.Converter
{
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
            ListConverter.ListWriteBytes(TypeArgs[0], stream, obj);
        }
    }

    [GenericConverter(typeof(Stack<>))]
    class StackConverter : GenericConverter
    {
        public override object GenericReadBytes(Stream stream)
        {
            object stack = Activator.CreateInstance(CurrentType);
            var stack_Push = CurrentType.GetMethod("Push");
            var count = stream.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                stack_Push.Invoke(stack, Serializer.Deserialize(TypeArgs[0], stream));
            }
            return stack;
        }

        public override void GenericWriteBytes(Stream stream, object obj)
        {
            var count = obj.GetMemberValue<int>("Count");
            stream.WriteInt32(count);
            var arr = (Array)CurrentType.GetMethod("ToArray").Invoke(obj);
            Array.Reverse(arr);
            foreach (var item in arr)
            {
                Serializer.Serialize(TypeArgs[0], item, stream);
            }
        }
    }

    [GenericConverter(typeof(Queue<>))]
    class QueueConverter : GenericConverter
    {

        public override object GenericReadBytes(Stream stream)
        {
            object queue = Activator.CreateInstance(CurrentType);
            var queue_Enqueue = CurrentType.GetMethod("Enqueue");

            int count = stream.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                queue_Enqueue.Invoke(queue, Serializer.Deserialize(TypeArgs[0], stream));
            }
            return queue;
        }

        public override void GenericWriteBytes(Stream stream, object obj)
        {
            ListConverter.ListWriteBytes(TypeArgs[0], stream, obj);
        }
    }


}
