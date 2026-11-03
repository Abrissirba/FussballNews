var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.FussballNews_ai_Web>("fussballnews-ai-web");

builder.Build().Run();
