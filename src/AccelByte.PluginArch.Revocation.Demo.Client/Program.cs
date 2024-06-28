// Copyright (c) 2023 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;

using CommandLine;

using AccelByte.Sdk.Core.Util;
using AccelByte.Sdk.Api.Platform.Operation;
using AccelByte.PluginArch.Revocation.Demo.Client.Model;

namespace AccelByte.PluginArch.Revocation.Demo.Client
{
    internal class Program
    {
        static int Main(string[] args)
        {
            int exitCode = 0;
            Parser.Default.ParseArguments<ApplicationConfig>(args)
                .WithParsed((config) =>
                {
                    config.FinalizeConfigurations();
                    PlatformWrapper wrapper = new PlatformWrapper(config);

                    Console.WriteLine($"\tBaseUrl: {config.BaseUrl}");
                    Console.WriteLine($"\tClientId: {config.ClientId}");
                    Console.WriteLine($"\tStore Category: {config.CategoryPath}");
                    if (config.GrpcServerUrl != "")
                        Console.WriteLine($"\tGrpc Target: {config.GrpcServerUrl}");
                    else if (config.ExtendAppName != "")
                        Console.WriteLine($"\tExtend App: {config.ExtendAppName}");
                    else
                    {
                        Console.WriteLine($"\tNO GRPC TARGET SERVER");
                        exitCode = 2;
                        return;
                    }

                    string userId = "";
                    try
                    {
                        Console.Write("Logging in to AccelByte... ");
                        wrapper.Login();
                        Console.WriteLine("[OK]");

                        Console.Write("Creating test user...");
                        var userInfo = wrapper.CreateTestUser();
                        userId = userInfo.UserId!;
                        Console.WriteLine($"User: {userInfo.Username}");
                        Console.WriteLine($"UserId: {userId}");

                        Console.Write("Configuring custom configuration... ");
                        wrapper.ConfigureGrpcTargetUrl();
                        Console.WriteLine("[OK]");
                        try
                        {
                            Console.Write("Check Currency... ");
                            wrapper.CheckAndCreateCurrencyIfNotExists();
                            Console.WriteLine("[OK]");

                            Console.Write("Check user's wallet...");
                            wrapper.CheckUserWalletBalance(userId);
                            Console.WriteLine("[OK]");

                            Console.Write("Creating draft store... ");
                            wrapper.CreateStore();
                            Console.WriteLine("[OK]");

                            Console.Write("Create store category... ");
                            wrapper.CreateCategory(config.CategoryPath);
                            Console.WriteLine("[OK]");

                            Console.Write("Create single item...");
                            List<SimpleItemInfo> items = wrapper.CreateItems(1, config.CategoryPath);
                            Console.WriteLine("[OK]");
                            items[0].WriteToConsole();

                            Console.Write("Publishing store changes... ");
                            wrapper.PublishStoreChange();
                            Console.WriteLine("[OK]");

                            try
                            {
                                Console.Write("Creating order for item...");
                                string orderNo = wrapper.CreateOrder(userId, items[0], 1);
                                Console.WriteLine("[OK]");
                                Console.WriteLine("OrderNo: {0}", orderNo);

                                Console.Write("Revoking order...");
                                var result = wrapper.RevokeOrder(userId, orderNo, items[0]);
                                Console.WriteLine("[OK]");
                                result.WriteToConsole();
                            }
                            catch (Exception x)
                            {
                                Console.WriteLine($"Exception: {x.Message}");
                                exitCode = 1;
                            }
                            finally
                            {
                                Console.Write("Removing item(s)... ");
                                wrapper.DeleteItems(items);
                                Console.WriteLine("[OK]");
                            }
                        }
                        catch (Exception x)
                        {
                            Console.WriteLine($"Exception: {x.Message}");
                            exitCode = 1;
                        }
                        finally
                        {
                            Console.Write("Deleting custom configuration... ");
                            wrapper.DeleteGrpcTargetUrl();
                            Console.WriteLine("[OK]");

                            Console.Write("Deleting store... ");
                            wrapper.DeleteStore();
                            Console.WriteLine("[OK]");
                        }
                    }
                    catch (Exception x)
                    {
                        Console.WriteLine($"Exception: {x.Message}");
                        exitCode = 1;
                    }
                    finally
                    {
                        if (userId != "")
                        {
                            Console.Write("Deleting test user...");
                            wrapper.DeleteUser(userId);
                            Console.WriteLine("[OK]");
                        }

                        wrapper.Logout();
                    }
                })
                .WithNotParsed((errors) =>
                {
                    Console.WriteLine("Invalid argument(s)");
                    foreach (var error in errors)
                        Console.WriteLine($"\t{error}");
                    exitCode = 2;
                });

            return exitCode;
        }
    }
}