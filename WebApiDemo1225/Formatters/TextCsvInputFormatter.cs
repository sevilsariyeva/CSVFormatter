using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WebApiDemo1225.Dtos;

namespace WebApiDemo1225.Formatters
{
    public class TextCsvInputFormatter : TextInputFormatter
    {
        public TextCsvInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanReadType(Type type)
            => type == typeof(StudentAddDto);

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(
            InputFormatterContext context, Encoding effectiveEncoding)
        {
            var httpContext = context.HttpContext;
            var serviceProvider = httpContext.RequestServices;

            var logger = serviceProvider.GetRequiredService<ILogger<VcardInputFormatter>>();

            using var reader = new StreamReader(httpContext.Request.Body, effectiveEncoding);

            try
            {
                var studentData = await reader.ReadToEndAsync();

                var students = ParseStudentData(studentData);

                return InputFormatterResult.Success(students);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error reading vCard input.");
                return InputFormatterResult.Failure();
            }
        }

        private StudentAddDto ParseStudent(string studentLine)
        {
            var parts = studentLine.Split('-');

            if (parts.Length != 5)
            {
                throw new FormatException("Invalid csv format");
            }

            return new StudentAddDto
            {
                Fullname = parts[1].Trim(),
                SeriaNo = parts[2].Trim(),
                Age = int.Parse(parts[3].Trim()),
                Score = double.Parse(parts[4].Trim())
            };
        }

        private StudentAddDto[] ParseStudentData(string input)
        {
            var lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var students = new StudentAddDto[lines.Length];

            for (int i = 0; i < lines.Length; i++)
            {
                students[i] = ParseStudent(lines[i]);
            }

            return students;
        }
    }
}
