using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.IO;
using System.Drawing;

namespace ParaBankTest.Utilities
{
    public static class ExcelHelper
    {
        public static void UpdateExcel(string filePath, string testCaseID, string actualResult, string status, string note)
        {
            // Thiết lập License EPPlus 8
            ExcelPackage.License.SetNonCommercialPersonal("Thuan");

            FileInfo fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists) return;

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                // Lấy Sheet "TestCase"
                ExcelWorksheet worksheet = package.Workbook.Worksheets["TestCase"] ?? package.Workbook.Worksheets[0];

                // Tìm dòng cuối cùng có dữ liệu
                int totalRows = worksheet.Dimension?.End.Row ?? 500;

                bool isFound = false;

                // Quét từ dòng 1 đến dòng cuối
                for (int row = 1; row <= totalRows; row++)
                {
                    // Lấy text tại cột 2 (Cột B - Nơi chứa TestCase ID trong file của bạn)
                    string cellValue = worksheet.Cells[row, 2].Text.Trim();

                    if (cellValue.Equals(testCaseID, StringComparison.OrdinalIgnoreCase))
                    {
                        // Ghi dữ liệu vào cột J(10), K(11), L(12)
                        worksheet.Cells[row, 10].Value = actualResult;
                        worksheet.Cells[row, 11].Value = status;
                        worksheet.Cells[row, 12].Value = note;

                        // Định dạng ô
                        var range = worksheet.Cells[row, 10, row, 12];
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        range.Style.WrapText = true;

                        // Tô màu Status
                        if (status.Equals("PASS", StringComparison.OrdinalIgnoreCase))
                            worksheet.Cells[row, 11].Style.Font.Color.SetColor(Color.Green);
                        else
                            worksheet.Cells[row, 11].Style.Font.Color.SetColor(Color.Red);

                        worksheet.Cells[row, 11].Style.Font.Bold = true;

                        isFound = true;
                        break; // Tìm thấy rồi thì thoát vòng lặp
                    }
                }

                if (!isFound)
                {
                    Console.WriteLine($"[ExcelHelper] Không tìm thấy mã {testCaseID} trong file Excel!");
                }

                package.Save();
            }
        }
    }
}