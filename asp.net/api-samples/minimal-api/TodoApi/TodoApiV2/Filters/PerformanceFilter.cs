using System;
using System.Diagnostics;

namespace TodoApiV2.Filters;

public class PerformanceFilter: IEndpointFilter
{
	protected readonly ILogger _logger;
	private readonly string _filterName;
	public PerformanceFilter(ILogger<PerformanceFilter> logger)
	{
		_logger = logger;
		_filterName = GetType().Name;
	}

	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
	{
	   
		var sw = Stopwatch.StartNew();
		_logger.LogInformation("Filter: {_filterName} before next", _filterName);
		var result = await next(context);
		sw.Stop();
		var path = context.HttpContext.Request.Path;
		_logger.LogInformation("Filter: {_filterName} after next", _filterName);
		_logger.LogInformation("Filter: {_filterName} - Endpoint con request path: {path} eseguito in {sw.ElapsedMilliseconds}ms", _filterName, path, sw.ElapsedMilliseconds);
		   return result;
	   
	}
}
