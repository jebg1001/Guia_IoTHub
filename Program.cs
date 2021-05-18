using Microsoft.Azure.Devices.Client;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Guia_IoTHub
{
    class Program
    {

        //Variable responsable de conectar con el IoT Hub
        private static DeviceClient s_deviceClient;

        //Variable responsable de declarar el tipo de transporte (en este caso siendo el protocolo MQTT).
        //El protocolo MQTT es por excelencia el protocolo utilizado para envíos de telemetría (Para mayor información visitar 'https://mqtt.org/')
        private static readonly TransportType s_transportType = TransportType.Mqtt;

        //Variable donde se almacena la cadena de conexión del dispositivo alojado en el IoT Hub
        private static string s_connectionString = "{Ingrese su cadena de conexión aquí}";

        static async Task Main(string[] args)
        {
            Console.WriteLine("IoT Hub - Simulador de dispositivo");
            Console.WriteLine("Creador: Juan Bejarano :)");

            // Creación del cliente de dispositivo usando el método CreateFromConnectionString el cual recibe dos parámetros: La cadena de conexión del dispositivo y el protocolo de transporte
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, s_transportType);

            // Creación de una finalización amigable del simulador
            Console.WriteLine("Presione control-C para finalizar la simulación.");

            // Declaración de la variable tipo CancellationTokenSource
            // Esta clase es la responsable de enviar la señal al programa de cancelar sus procesos
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Finalizando simulación...");
            };

            // Llamado al método EnvioMensajesNubeAsync en forma de bucle, este método recibe un parámetro: El Token de cancelación
            await EnvioMensajesNubeAsync(cts.Token);

            //Método para desechar el cliente de dispositivo una vez finalizada la simulación
            s_deviceClient.Dispose();
            Console.WriteLine("Device simulator finished.");
        }

        // Método asíncrono para el envio de telemetría al dispositivo alojado en el IoT Hub
        private static async Task EnvioMensajesNubeAsync(CancellationToken ct)
        {
            // Declaración de valores mínimos recibidos en la telemetría (en este ejemplo se enviarán datos simulados de Temperatura y Humedad)
            double minTemperatura = 20;
            double minHumedad = 60;
            var rand = new Random();

            // Bucle para la creación y envío de datos el cual puede ser interrumpido si el Token de cancelación recibe el llamado de cancelación
            while (!ct.IsCancellationRequested)
            {
                // Generación aleatoria de un nuevo valor de Temperatura y Humedad
                double nuevaTemperatura = minTemperatura + rand.NextDouble() * 15;
                double nuevaHumedad = minHumedad + rand.NextDouble() * 20;

                // Creación del mensaje en formato JSON
                // El formato JSON es el formato aceptado para envios y lecturas de mansajes en IoT Hub
                // Para mayor información visitar "https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-messages-construct"
                string messageBody = JsonSerializer.Serialize(
                    new
                    {
                        temperatura = nuevaTemperatura,
                        humedad = nuevaHumedad,
                    });
                using var mensaje = new Message(Encoding.ASCII.GetBytes(messageBody))
                {
                    ContentType = "application/json",
                    ContentEncoding = "utf-8",
                };

                // Envío del mensaje de telemetría de forma asíncrona
                await s_deviceClient.SendEventAsync(mensaje);
                Console.WriteLine($"{DateTime.Now} > Enviando mensaje: {messageBody}");

                // Método para declarar un intervalo en la iteración del bucle (1000 = 1 segundo) 
                await Task.Delay(1000);
            }
        }
    }
}
