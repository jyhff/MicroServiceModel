# LCH.Abp.BackgroundTasks.Jobs

Common job module for background tasks (queue).

## Job List

* [ConsoleJob](./LCH/Abp/BackgroundTasks/Jobs/ConsoleJob):                   Console output
* [HttpRequestJob](./LCH/Abp/BackgroundTasks/Jobs/HttpRequestJob):           HTTP request
* [SendEmailJob](./LCH/Abp/BackgroundTasks/Jobs/SendEmailJob):               Send email
* [SendSmsJob](./LCH/Abp/BackgroundTasks/Jobs/SendSmsJob):                  Send SMS
* [ServiceInvocationJob](./LCH/Abp/BackgroundTasks/Jobs/ServiceInvocationJob): Service invocation (HTTP request extension)
* [SleepJob](./LCH/Abp/BackgroundTasks/Jobs/SleepJob):                      Sleep, delay job execution

## Configuration and Usage

Module reference:

```csharp
[DependsOn(typeof(AbpBackgroundTasksJobsModule))]
public class YouProjectModule : AbpModule
{
  // other
}
```

## Features

### Console Job
- Output messages to console with timestamp
- Configurable message content

### HTTP Request Job
- Support for various HTTP methods (GET, PUT, POST, PATCH, OPTIONS, DELETE)
- Custom headers and content type
- Request data handling
- Culture support

### Email Job
- Send emails with customizable:
  - Recipients
  - Subject
  - From address
  - Body content
  - Email templates
  - Template model and context
  - Culture support

### SMS Job
- Send SMS messages with:
  - Phone number targeting
  - Message content
  - Custom properties

### Service Invocation Job
- Extended HTTP request functionality
- Support for different providers (http, dapr)
- Multi-tenant support
- Service name configuration
- Dapr integration with App ID support

### Sleep Job
- Delay job execution
- Configurable delay duration in milliseconds
- Default 20-second delay if not specified
