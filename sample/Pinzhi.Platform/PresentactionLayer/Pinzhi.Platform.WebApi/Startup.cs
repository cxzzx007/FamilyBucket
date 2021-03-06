﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlSugar;
using Bucket.DbContext;
using Newtonsoft.Json.Serialization;
using Bucket.AspNetCore.Filters;
using Bucket.AspNetCore.Extensions;
using AutoMapper;
using Pinzhi.Platform.Interface;
using Pinzhi.Platform.Business;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using Bucket.AspNetCore.EventBus;
using Bucket.Logging;
using System;
using Bucket.Utility;

namespace Pinzhi.Platform.WebApi
{
    /// <summary>
    /// 启动配置
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 初始化启动配置
        /// </summary>
        /// <param name="configuration">配置</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        /// <summary>
        /// 配置
        /// </summary>
        public IConfiguration Configuration { get; }
        /// <summary>
        /// 配置服务
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            // 添加授权认证
            services.AddBucketAuthentication(Configuration);
            // 添加基础设施服务
            services.AddBucket();
            // 添加数据ORM
            services.AddSQLSugarClient<SqlSugarClient>(config => {
                config.ConnectionString = Configuration.GetSection("SqlSugarClient")["ConnectionString"];
                config.DbType = DbType.MySql;
                config.IsAutoCloseConnection = false;
                config.InitKeyType = InitKeyType.Attribute;
            });
            // 添加错误码服务
            services.AddErrorCodeService(Configuration);
            // 添加配置服务
            services.AddConfigService(Configuration);
            // 添加事件驱动
            var eventConfig = Configuration.GetSection("EventBus").GetSection("RabbitMQ");
            services.AddEventBus(option =>
            {
                option.UseRabbitMQ(opt =>
                {
                    opt.HostName = eventConfig["HostName"];
                    opt.Port = Convert.ToInt32(eventConfig["Port"]);
                    opt.ExchangeName = eventConfig["ExchangeName"];
                    opt.QueueName = eventConfig["QueueName"];
                });
            });
            // 添加服务发现
            services.AddServiceDiscoveryConsul(Configuration);
            // 添加事件队列日志
            services.AddEventLog();
            // 添加链路追踪
            services.AddTracer(Configuration);
            // 添加模型映射,需要映射配置文件(考虑到性能未使用自动映射)
            services.AddAutoMapper();
            // 添加业务注册
            #region
            services.AddScoped<IMenuBusiness, MenuBusiness>();
            services.AddScoped<IPlatformBusiness, PlatformBusiness>();
            services.AddScoped<IProjectBusiness, ProjectBusiness>();
            services.AddScoped<IRoleBusiness, RoleBusiness>();
            services.AddScoped<IUserBusiness, UserBusiness>();
            services.AddScoped<IApiBusiness, ApiBusiness>();
            #endregion
            // 添加过滤器
            services.AddMvc(options =>
            {
               options.Filters.Add(typeof(WebApiTraceFilterAttribute));
               options.Filters.Add(typeof(WebApiActionFilterAttribute));
            }).AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            });
            // 添加Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "品值POC接口文档", Version = "v1" });
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Pinzhi.Platform.WebApi.xml"));
            });
            // 添加工具
            services.AddUtil();
        }
        /// <summary>
        /// 配置请求管道
        /// </summary>
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            // 日志
            loggerFactory.AddBucketLog(app, "Pinzhi.Platform");
            // 文档
            ConfigSwagger(app);
            // 公共配置
            CommonConfig(app);
        }
        /// <summary>
        /// 配置Swagger
        /// </summary>
        private void ConfigSwagger(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "api v1");
            });
        }
        /// <summary>
        /// 公共配置
        /// </summary>
        private void CommonConfig(IApplicationBuilder app)
        {
            // 认证授权
            app.UseAuthentication();
            // 链路追踪
            app.UseTracer();
            // 全局错误日志
            app.UseErrorLog();
            // 静态文件
            app.UseStaticFiles();
            // 路由
            ConfigRoute(app);
            // 服务注册
            app.UseConsulRegisterService(Configuration);
        }
        /// <summary>
        /// 路由配置,支持区域
        /// </summary>
        private void ConfigRoute(IApplicationBuilder app)
        {
            app.UseMvc(routes => {
                routes.MapRoute("areaRoute", "view/{area:exists}/{controller}/{action=Index}/{id?}");
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
                routes.MapSpaFallbackRoute("spa-fallback", new { controller = "Home", action = "Index" });
            });
        }
    }
}
