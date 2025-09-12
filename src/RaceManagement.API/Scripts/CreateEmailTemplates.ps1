# PowerShell script to create email template directories and files

$templatesPath = "EmailTemplates"
New-Item -ItemType Directory -Path $templatesPath -Force

# Create template files with basic content
@{
    "registration-confirmation" = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>Xác nhận đăng ký</title>
</head>
<body style="font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5;">
    <div style="max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px;">
        <h1 style="color: #2563eb; text-align: center;">Xác nhận đăng ký thành công!</h1>
        <p>Chào <strong>{{FullName}}</strong>,</p>
        <p>Chúc mừng bạn đã đăng ký thành công <strong>{{RaceName}}</strong>!</p>
        
        <div style="background: #f8fafc; padding: 15px; border-radius: 5px; margin: 20px 0;">
            <h3>Thông tin đăng ký:</h3>
            <ul>
                <li><strong>Họ tên:</strong> {{FullName}}</li>
                <li><strong>Cự ly:</strong> {{Distance}}</li>
                <li><strong>Mã tham chiếu:</strong> {{TransactionReference}}</li>
                <li><strong>Số tiền:</strong> {{Price}} VNĐ</li>
                <li><strong>Thông tin áo:</strong> {{ShirtInfo}}</li>
            </ul>
        </div>
        
        <div style="background: #fef3c7; padding: 15px; border-radius: 5px; margin: 20px 0;">
            <h3>Thanh toán:</h3>
            <p>Vui lòng chuyển khoản với nội dung: <strong>{{TransactionReference}}</strong></p>
            <p>Sau khi thanh toán, bạn sẽ nhận được email thông báo số BIB.</p>
        </div>
        
        <p>Cảm ơn bạn đã tham gia!</p>
        <p>{{CompanyName}}</p>
    </div>
</body>
</html>
"@
} | ForEach-Object {
    $_.GetEnumerator() | ForEach-Object {
        Set-Content -Path "$templatesPath\$($_.Key).html" -Value $_.Value
    }
}

Write-Host "Email templates created successfully in $templatesPath"