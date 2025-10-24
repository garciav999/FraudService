using System.Net;
using System.Text;
using Newtonsoft.Json;
using Application.DTOs;
using Application.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Lambda
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var port = 5051;
            var listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();

            Console.WriteLine($"🚀 Anti-Fraud Service started on: http://localhost:{port}/");
            Console.WriteLine("📋 Available endpoints:");
            Console.WriteLine("   POST /upsert-transaction-day - Test transaction day upsert");
            Console.WriteLine("   POST /analyze-fraud - Test fraud analysis");
            Console.WriteLine("   POST /start-kafka-consumer - Start Kafka consumer manually");
            Console.WriteLine();

            var function = new Function();
            var fraudAnalysisFunction = new FraudAnalysisFunction();
            var kafkaConsumerFunction = new KafkaConsumerFunction();

            // 🔥 INICIAR KAFKA CONSUMER AUTOMÁTICAMENTE
            Console.WriteLine("🔄 Starting Kafka consumer automatically...");
            var consumerCancellationTokenSource = new CancellationTokenSource();
            
            _ = Task.Run(async () =>
            {
                try
                {
                    // Usar la función lambda existente que ya maneja los scopes correctamente
                    var result = await kafkaConsumerFunction.StartConsumer();
                    Console.WriteLine($"✅ Kafka consumer result: {result.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Kafka Consumer Error: {ex.Message}");
                    Console.WriteLine($"   Details: {ex.StackTrace}");
                }
            });

            Console.WriteLine("⏳ Waiting for HTTP requests and Kafka events... (Press Ctrl+C to stop)\n");

            while (true)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    _ = Task.Run(async () => await HandleRequest(context, function, fraudAnalysisFunction, kafkaConsumerFunction));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Server Error: {ex.Message}");
                }
            }
        }

        static async Task HandleRequest(
            HttpListenerContext context, 
            Function function, 
            FraudAnalysisFunction fraudAnalysisFunction,
            KafkaConsumerFunction kafkaConsumerFunction)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                Console.WriteLine($"\n🔔 New Request - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"   Method: {request.HttpMethod}");
                Console.WriteLine($"   URL: {request.Url?.PathAndQuery}");

                if (request.HttpMethod != "POST")
                {
                    await SendResponse(response, 405, new { error = "Only POST method allowed" });
                    Console.WriteLine("   ❌ 405 - Method Not Allowed");
                    return;
                }

                string body = "";
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    body = await reader.ReadToEndAsync();
                }

                Console.WriteLine($"   Body: {body}");

                var path = request.Url?.AbsolutePath?.ToLower() ?? "";

                switch (path)
                {
                    case "/upsert-transaction-day":
                        await HandleUpsertTransactionDay(response, body, function);
                        break;

                    case "/analyze-fraud":
                        await HandleFraudAnalysis(response, body, fraudAnalysisFunction);
                        break;

                    case "/start-kafka-consumer":
                        await HandleKafkaConsumer(response, kafkaConsumerFunction);
                        break;

                    default:
                        await SendResponse(response, 404, new { 
                            error = "Endpoint not found",
                            availableEndpoints = new[] {
                                "/upsert-transaction-day",
                                "/analyze-fraud", 
                                "/start-kafka-consumer"
                            }
                        });
                        Console.WriteLine("   ❌ 404 - Endpoint Not Found");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error: {ex.Message}");
                await SendResponse(response, 500, new { 
                    error = ex.Message, 
                    type = ex.GetType().Name,
                    stackTrace = ex.StackTrace
                });
            }
        }

        static async Task HandleUpsertTransactionDay(HttpListenerResponse response, string body, Function function)
        {
            try
            {
                var lambdaRequest = JsonConvert.DeserializeObject<Function.UpsertTransactionDayRequest>(body);
                
                if (lambdaRequest == null)
                {
                    await SendResponse(response, 400, new { error = "Invalid JSON for UpsertTransactionDayRequest" });
                    Console.WriteLine("   ❌ 400 - Invalid JSON");
                    return;
                }

                var result = await function.Handler(lambdaRequest);
                await SendResponse(response, 200, result);
                Console.WriteLine("   ✅ 200 - UpsertTransactionDay Success");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in HandleUpsertTransactionDay: {ex.Message}", ex);
            }
        }

        static async Task HandleFraudAnalysis(HttpListenerResponse response, string body, FraudAnalysisFunction fraudAnalysisFunction)
        {
            try
            {
                var analysisRequest = JsonConvert.DeserializeObject<FraudAnalysisFunction.FraudAnalysisRequest>(body);
                
                if (analysisRequest == null)
                {
                    await SendResponse(response, 400, new { error = "Invalid JSON for FraudAnalysisRequest" });
                    Console.WriteLine("   ❌ 400 - Invalid JSON");
                    return;
                }

                var result = await fraudAnalysisFunction.AnalyzeTransaction(analysisRequest);
                await SendResponse(response, 200, result);
                Console.WriteLine("   ✅ 200 - FraudAnalysis Success");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in HandleFraudAnalysis: {ex.Message}", ex);
            }
        }

        static async Task HandleKafkaConsumer(HttpListenerResponse response, KafkaConsumerFunction kafkaConsumerFunction)
        {
            try
            {
                // Ejecutar en background para no bloquear la respuesta
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await kafkaConsumerFunction.StartConsumer();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Kafka Consumer Error: {ex.Message}");
                    }
                });

                await SendResponse(response, 200, new { 
                    message = "Kafka consumer started in background",
                    note = "Check console for consumer logs"
                });
                Console.WriteLine("   ✅ 200 - Kafka Consumer Started");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in HandleKafkaConsumer: {ex.Message}", ex);
            }
        }

        static async Task SendResponse(HttpListenerResponse response, int statusCode, object data)
        {
            try
            {
                response.StatusCode = statusCode;
                response.ContentType = "application/json";
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
                
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                var bytes = Encoding.UTF8.GetBytes(json);
                await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            }
            finally
            {
                response.Close();
            }
        }
    }
}