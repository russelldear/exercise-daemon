using System;
using AspNetCore.OAuth.Provider.Strava;
using ExerciseDaemon.Signup.ExternalServices;
using ExerciseDaemon.Signup.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ExerciseDaemon.Signup
{
    public class Startup
    {
        private readonly ISubstitutionBinder _substitutionBinder;

        public Startup(IConfiguration configuration, ISubstitutionBinder substitutionBinder)
        {
            _substitutionBinder = substitutionBinder;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var stravaSettings = _substitutionBinder.BuildStravaSettings(services);
            services.TryAddSingleton(stravaSettings);

            var dynamoDbSettings = _substitutionBinder.BuildDynamoDbSettings(services);
            services.TryAddSingleton(dynamoDbSettings);

            var slackSettings = _substitutionBinder.BuildSlackSettings(services);
            services.TryAddSingleton(slackSettings);
            
            services.TryAddSingleton<StravaService>();
            services.TryAddSingleton(new AthleteRepository(dynamoDbSettings));
            services.TryAddSingleton(new SlackService(slackSettings));

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddStrava(options =>
            {
                options.ClientId = stravaSettings.ClientId.ToString();
                options.ClientSecret = stravaSettings.ClientSecret;
                options.Scope.Remove("public");
                options.Scope.Add("read");
                options.Scope.Add("activity:read");
                options.Scope.Add("activity:read_all");
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
