using AspNetCore.OAuth.Provider.Strava;
using ExerciseDaemon.ExternalServices;
using ExerciseDaemon.Helpers;
using ExerciseDaemon.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ExerciseDaemon
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
            var stravaSettings = _substitutionBinder.BuildStravaSettings();
            services.TryAddSingleton(stravaSettings);

            var dynamoDbSettings = _substitutionBinder.BuildDynamoDbSettings();
            services.TryAddSingleton(dynamoDbSettings);

            var slackSettings = _substitutionBinder.BuildSlackSettings();
            services.TryAddSingleton(slackSettings);

            services.TryAddSingleton<AthleteRepository>();
            services.TryAddSingleton<StravaService>();
            services.TryAddSingleton<SlackService>();
            services.TryAddSingleton<StatementRandomiser>();

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
                options.Scope.Add("profile:read_all");
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
