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

			services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/Login");

			services.AddAuthentication(IdentityConstants.ApplicationScheme)
				.AddCookie(options => {
					options.LoginPath = "/Account/LogIn";
					options.LogoutPath = "/Account/LogOff";
				});

			// Services used by identity
			//services.AddAuthentication(options =>
			//{
			//	// This is the Default value for ExternalCookieAuthenticationScheme
			//	options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
			//	options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
			//	options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
			//});

			services.AddOptions();
			services.AddDataProtection();

			//services.TryAddSingleton<IdentityMarkerService>();
			//services.TryAddSingleton<IUserValidator<ElasticUser>, UserValidator<ElasticUser>>();
			//services.TryAddSingleton<IPasswordValidator<ElasticUser>, PasswordValidator<ElasticUser>>();
			services.TryAddSingleton<IPasswordHasher<ElasticUser>, PasswordHasher<ElasticUser>>();
			services.TryAddSingleton<ILookupNormalizer, UpperInvariantLookupNormalizer>();
			//services.TryAddSingleton<IdentityErrorDescriber>();
			services.TryAddSingleton<ISecurityStampValidator, SecurityStampValidator<ElasticUser>>();
			//services.TryAddSingleton<IUserClaimsPrincipalFactory<ElasticUser>, UserClaimsPrincipalFactory<ElasticUser>>();
			//services.TryAddSingleton<UserManager<ElasticUser>, UserManager<ElasticUser>>();
			//services.TryAddScoped<SignInManager<ElasticUser>, SignInManager<ElasticUser>>();

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
				app.UseDatabaseErrorPage();
				app.UseBrowserLink();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			//app.UseIdentity();
			app.UseAuthentication();

			// Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});

		}
			
		public class UserClaimsPrincipalFactory<TUser> : IUserClaimsPrincipalFactory<TUser>
			where TUser : class
		{
			public UserClaimsPrincipalFactory(
				UserManager<TUser> userManager,
				IOptions<IdentityOptions> optionsAccessor)
			{
				if (optionsAccessor?.Value == null)
				{
					throw new ArgumentNullException(nameof(optionsAccessor));
				}

				UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
				Options = optionsAccessor.Value;
			}

			public UserManager<TUser> UserManager { get; }

			public IdentityOptions Options { get; }

			public virtual async Task<ClaimsPrincipal> CreateAsync(TUser user)
			{
				if (user == null)
				{
					throw new ArgumentNullException(nameof(user));
				}

				var userId = await UserManager.GetUserIdAsync(user);
				var userName = await UserManager.GetUserNameAsync(user);

				//var id = new ClaimsIdentity(Options.Cookies.ApplicationCookieAuthenticationScheme,
				//	Options.ClaimsIdentity.UserNameClaimType,
				//	Options.ClaimsIdentity.RoleClaimType);

				var id = new ClaimsIdentity(IdentityConstants.ApplicationScheme);

				id.AddClaim(new Claim(Options.ClaimsIdentity.UserIdClaimType, userId));
				id.AddClaim(new Claim(Options.ClaimsIdentity.UserNameClaimType, userName));
				if (UserManager.SupportsUserSecurityStamp)
				{
					id.AddClaim(new Claim(Options.ClaimsIdentity.SecurityStampClaimType,
						await UserManager.GetSecurityStampAsync(user)));
				}
				if (UserManager.SupportsUserRole)
				{
					var roles = await UserManager.GetRolesAsync(user);
					foreach (var roleName in roles)
					{
						id.AddClaim(new Claim(Options.ClaimsIdentity.RoleClaimType, roleName));
					}
				}
				if (UserManager.SupportsUserClaim)
				{
					id.AddClaims(await UserManager.GetClaimsAsync(user));
				}

				return new ClaimsPrincipal(id);
			}
		}

	}
}
