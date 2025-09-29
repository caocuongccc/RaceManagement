using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Utilities.Zlib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Infrastructure.Data
{
    internal class ClearDB
    {
        //-- Xoá dữ liệu theo thứ tự phụ thuộc
        /*
        DELETE FROM Payments;
        DELETE FROM EmailLogs;
        DELETE FROM EmailQueues;
        DELETE FROM Registrations;
        DELETE FROM RaceDistances;
        DELETE FROM RaceShirtTypes;
        DELETE FROM Races;
        DELETE FROM GoogleSheetConfigs;
        DELETE FROM GoogleCredentials;

        -- Reset Identity về 1
        DBCC CHECKIDENT('Payments', RESEED, 0);
        DBCC CHECKIDENT('EmailLogs', RESEED, 0);
        DBCC CHECKIDENT('EmailQueues', RESEED, 0);
        DBCC CHECKIDENT('Registrations', RESEED, 0);
        DBCC CHECKIDENT('RaceDistances', RESEED, 0);
        DBCC CHECKIDENT('RaceShirtTypes', RESEED, 0);
        DBCC CHECKIDENT('Races', RESEED, 0);
        DBCC CHECKIDENT('GoogleSheetConfigs', RESEED, 0);
        DBCC CHECKIDENT('GoogleCredentials', RESEED, 0);
        */
        /*
        cd src/RaceManagement.API
        dotnet ef migrations add InitialCreate --project../RaceManagement.Infrastructure --startup-project.
        dotnet ef database update --project../RaceManagement.Infrastructure --startup-project.

        dotnet ef migrations add AddRaceBankInfoAndRegistrationFee
        dotnet Ef database update

        */

        /* 
            🏁 Checklist test hệ thống Race Management
            1. Test API Credentials & SheetConfig
                Vào Swagger (/swagger/index.html) của API.
                Gọi POST /api/credentials → upload file Google service account JSON.
                Gọi POST /api/sheetconfigs → tạo sheet config (SpreadsheetId, SheetName, CredentialId vừa tạo).
                Gọi GET /api/sheetconfigs/test/{id} → test connection thành công.
                👉 Nếu test pass, bạn đã kết nối được Google Sheets.
            2. Test tạo Race
                Trong Swagger, gọi POST /api/races với payload kiểu CreateRaceDto.
                Gắn SheetConfigId đúng ở trên.
                Thêm 1–2 cự ly (Distances).
                Thêm 1–2 loại áo (ShirtTypes).
                👉 Nếu thành công, gọi GET /api/races sẽ thấy race mới.
            3. Test đồng bộ VĐV từ Google Sheet
                Thêm vài dòng test vào Google Form / Google Sheet gốc.
                Trong Swagger gọi POST /api/registrations/sync/{raceId}.
                Xem response: số lượng Added / Skipped / Errors.
                Gọi GET /api/registrations/by-race/{raceId} → thấy danh sách VĐV trong DB.
            4. Test gửi email (Hangfire)
                Mở Hangfire dashboard ở /hangfire.
                Khi sync VĐV, hệ thống có enqueue job SendRegistrationConfirmationEmailAsync.
                Vào dashboard xem job có chạy không.
                Nếu mail config đúng, VĐV sẽ nhận email có QR.
            5. Test Dashboard MVC
                Chạy RaceManagement.Web → login.
                Vào /dashboard.
                Xem thống kê: số race, số VĐV, trạng thái Paid/Unpaid.
                Click vào race → xem danh sách Registration.
                Export danh sách ra Excel → kiểm tra file.
            6. Test Payment flow
                Trong Google Sheet Payment, đánh dấu X cho 1 VĐV.
                Gọi API sync lại → hệ thống update PaymentStatus = Paid.
                Hangfire queue job gửi mail BIB.
                Kiểm tra VĐV có BibNumber và nhận mail.
            ✅ Nếu pass hết 6 bước này → hệ thống end-to-end đã chạy.
        
         */



    }
}
