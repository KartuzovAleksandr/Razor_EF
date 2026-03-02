var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Razor_EF>("razor-ef");

builder.Build().Run();
