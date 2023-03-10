using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Universe
{
    public static class XmlUtility
    {
        public const string EXTENSION_XML = ".xml";

        static readonly Dictionary<Type, XmlSerializer> s_XmlSerializers = new();
        
        /// <summary>
        /// 保存到xml
        /// </summary>
        /// <param name="data"></param>
        /// <param name="path"></param>
        public static void SerializeXml(this object data, string path)
        {
            if (string.IsNullOrEmpty(path) || data == null)
            {
                return;
            }

            string directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }

            if (!string.IsNullOrEmpty(directory))
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }

            using (StreamWriter stream = new(path))
            {
                XmlSerializer serializer = GetXmlSerializer(data.GetType());
                serializer.Serialize(stream, data);
            }
        }

        /// <summary>
        /// 写入XML
        /// </summary>
        /// <param name="data"></param>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        public static void SerializeXml<T>(T data, string path) where T : class
        {
            if (string.IsNullOrEmpty(path) || data == null)
            {
                return;
            }

            string directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }

            if (!string.IsNullOrEmpty(directory))
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }

            using (StreamWriter stream = new(path))
            {
                XmlSerializer serializer = GetXmlSerializer<T>();
                serializer.Serialize(stream, data);
            }
        }

        /// <summary>
        /// 从Xml加载对象，没有则会创建xml
        /// </summary>
        /// <param name="absPath">绝对路径</param>
        /// <param name="autoCreate">是否自动创建</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T DeserializeXml<T>(string absPath, bool autoCreate = true) where T : class, new()
        {
            if (string.IsNullOrEmpty(absPath))
            {
                return null;
            }

            XmlSerializer serializer = GetXmlSerializer<T>();
            if (autoCreate)
            {
                string directory = Path.GetDirectoryName(absPath);
                if (directory != null)
                {
                    //检查文件夹
                    if (!File.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    //检查文件
                    if (!File.Exists(absPath))
                    {
                        using (StreamWriter stream = new(absPath))
                        {
                            serializer.Serialize(stream, new T());
                        }
                    }
                }
            }

            T ret = null;
            using (StreamReader stream = new(absPath))
            {
                ret = serializer.Deserialize(stream) as T;
            }

            return ret;
        }

        public static XmlSerializer GetXmlSerializer(Type type)
        {
            if (type == null)
            {
                return default;
            }

            XmlSerializer serializer;
            if (s_XmlSerializers.ContainsKey(type))
            {
                serializer = s_XmlSerializers[type];
            }
            else
            {
                serializer = new(type);
                s_XmlSerializers[type] = serializer;
            }

            return serializer;
        }

        /// <summary>
        /// 获取对象Xml序列化器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static XmlSerializer GetXmlSerializer<T>()
        {
            return GetXmlSerializer(typeof(T));
        }
    }
}