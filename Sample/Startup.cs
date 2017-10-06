using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sample.Models;
using Sample.Services;
using Elasticsearch.Net;
using Nest;
using AspNetCore.Identity.ElasticSearch;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using System.IO;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Sample
{
	public class Startup
	{

		private IHostingEnvironment _environment = null;

		public Startup(IHostingEnvironment env)
		{

			_environment = env;

			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

			if (env.IsDevelopment())
			{
				// For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
				builder.AddUserSecrets<Startup>();
			}

			builder.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; }
		

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Configuration
			services.Configure<ElasticOptions>(Configuration.GetSection("ElasticSearchIdentity"));

			// ES Registration
			services.AddSingleton<IElasticClient>(sp =>
			{
				var esUri = new Uri(Configuration["ElasticSearchUri"]);
				var esConnectionConfiguration = new ConnectionSettings(new StaticConnectionPool(new List<Uri> { esUri })
				{
					SniffedOnStartup = false,
				});
				esConnectionConfiguration.ThrowExceptions(true);
				esConnectionConfiguration.DisableDirectStreaming(true);
				var nestClient = new ElasticClient(esConnectionConfiguration);
				return nestClient;
			});			

			

			services.AddIdentity<ElasticUser, ElasticRole>()
				.AddElasticsearchIdentity()
				.AddDefaultTokenProviders();

			services.Configure<IdentityOptions>(options =>
			{
				options.Lockout.AllowedForNewUsers = true;
			});

			services.AddOptions();
			services.AddDataProtection();
			
			services.AddMvc();

			// Add application services.
			services.AddTransient<IEmailSender, AuthMessageSender>();
			services.AddTransient<ISmsSender, AuthMessageSender>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseBrowserLink();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();
			
			app.UseAuthentication();

			// Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});

		}
	}
}
