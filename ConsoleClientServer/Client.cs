﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using DataBaseType;
using SunflowerDB;

namespace ConsoleClientServer
{
    public interface IClient
    {
        void Connect();
        void Dispose();
        void SendResieveMessage<T>();
        string ConvertMessageToString<T>(T value);
    }

    public abstract class Client : IDisposable, IClient
    {
        private readonly string _host;
        private readonly int _port;
        private readonly TcpClient _client;
        private NetworkStream _stream;
        private readonly Thread _receiveThread;
        private bool _disposed = false;
        private bool _stopworking = false;

        public Client(string host, int port)
        {
            _host = host;
            _port = port;
            _client = new TcpClient();
        }
        public void Connect()
        {
            var tries = 0;
            var connected = false;

            while (!connected)
            {
                try
                {
                    _client.Connect(_host, _port); //подключение клиента
                    _stream = _client.GetStream(); // получаем поток
                    connected = true;
                    Console.WriteLine($"Conection to {_host}:{_port} established");
                }
                catch (Exception)
                {

                    tries++;
                    System.Threading.Thread.Sleep(500);
                    Console.WriteLine($"Conection to {_host}:{_port} failed");
                    if (tries == 5)
                    {
                        Console.WriteLine("Try again?(Y/N)");

                        if (Console.ReadLine().Trim().ToLower() == "y")
                        {
                            tries = 0;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        // отправка сообщений
        public abstract string ConvertMessageToString<T>(T value);

        public void SendResieveMessage<T>()
        {
            if (_stream == null)
            {
                Connect();
            }

            while (!_stopworking)
            {
                try
                {
                    {
                        var message = Console.ReadLine();
                        var data = Encoding.Unicode.GetBytes(message);
                        _stream.Write(data, 0, data.Length);
                    }
                    {
                        var data = new byte[64]; // буфер для получаемых данных
                        var bytes = 0;
                        using var binaryData = new MemoryStream();
                        do
                        {
                            bytes = _stream.Read(data, 0, data.Length);
                            binaryData.Write(data, 0, bytes);
                        }
                        while (_stream.DataAvailable);

                        var formatter = new BinaryFormatter();
                        var value = (T)formatter.Deserialize(binaryData);
                        var result = ConvertMessageToString<T>(value);
                        Console.WriteLine(result);//вывод сообщения
                    }
                }
                catch
                {
                    Console.WriteLine("Подключение прервано!"); //соединение было прервано
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        // получение сообщений

        private void Disconnect()
        {
            if (_stream != null)
            {
                _stream.Close();//отключение потока
            }

            if (_client != null)
            {
                _client.Close();//отключение клиента
            }

            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            _stopworking = true;

            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _client?.Dispose();
                _stream?.Dispose();
                _receiveThread?.Join();
            }

            _disposed = true;
        }

        ~Client()
        {
            Dispose(false);
        }
    }
}
