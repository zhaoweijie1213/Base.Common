using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Protobuf.Extension
{
    public static class ProtobufExtension
    {
        /// <summary>
        /// 对象转换成二进制流
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static byte[] ObjectToProbuf<T>(this T model)
        {
            using MemoryStream stream = new MemoryStream();
            ProtoBuf.Serializer.Serialize(stream, model);

            byte[] buffer = stream.ToArray();

            return buffer;

        }


        /// <summary>
        /// byte[]转换成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="buffer"></param>
        public static T ProtobufToObject<T>(this byte[] buffer)
        {
            using MemoryStream stream = new MemoryStream(buffer);
            return ProtoBuf.Serializer.Deserialize<T>(stream);

        }
    }
}
