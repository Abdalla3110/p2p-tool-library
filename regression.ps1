# Full regression run — all routes, all roles (5-controller layout)
$base = "http://localhost:5080"
$results = @()

function Login($email, $pass) {
    $s = $null
    $r = Invoke-WebRequest -Uri "$base/Home/Login" -SessionVariable s -UseBasicParsing
    $token = ([regex]'name="__RequestVerificationToken" type="hidden" value="([^"]+)"').Match($r.Content).Groups[1].Value
    try {
        Invoke-WebRequest -Uri "$base/Home/Login" -Method Post -Body @{__RequestVerificationToken=$token; Email=$email; Password=$pass} -WebSession $s -UseBasicParsing -MaximumRedirection 0 -ErrorAction Stop | Out-Null
    } catch { }
    return $s
}

function Hit($session, $url, $role, $expected = 200) {
    try {
        $x = Invoke-WebRequest -Uri "$base$url" -WebSession $session -UseBasicParsing -MaximumRedirection 5 -TimeoutSec 30
        $code = $x.StatusCode
    } catch {
        $code = $_.Exception.Response.StatusCode.value__
        if (-not $code) { $code = "ERR" }
    }
    $script:results += [pscustomobject]@{ Role = $role; Route = $url; Status = $code }
}

Start-Sleep -Seconds 2

# ---- Public (no auth) ----
$pub = @('/', '/Home/Tools', '/Home/Details/1', '/Home/Details/3', '/Home/PublicProfile/4', '/Home/Availability/1',
         '/Home/Login', '/Home/Register', '/Home/ForgotPassword', '/health')
foreach ($u in $pub) { Hit $null $u "Public" }

# ---- Unauthorized checks ----
foreach ($u in @('/Member', '/Library', '/Maintenance', '/Admin', '/Member/Details/1', '/Member/Messages')) { Hit $null $u "Guest→Login" }

# ---- Member ----
$member = Login 'test@example.com' 'NewPass@123'
$memberRoutes = @('/Member', '/Member/Tools', '/Member/ToolCreate', '/Member/Bookings', '/Member/Wallet',
    '/Member/Notifications', '/Member/Profile', '/Member/Ratings', '/Member/Referral',
    '/Member/Messages', '/Member/MyDisputes', '/Member/MyClaims',
    '/Member/Details/1', '/Member/Details/2', '/Member/Details/4', '/Member/Checkout/4')
foreach ($u in $memberRoutes) { Hit $member $u "Member" }

# ---- Librarian ----
$lib = Login 'librarian@toolshare.local' 'Lib@12345'
$libRoutes = @('/Library', '/Library/Tasks', '/Library/Disputes', '/Library/Disputes/1', '/Library/Audits',
    '/Library/Handovers', '/Library/Claims', '/Library/LateReturns', '/Library/Blacklist',
    '/Library/Tools', '/Library/Users', '/Library/Bookings', '/Library/Broadcasts', '/Library/Zones')
foreach ($u in $libRoutes) { Hit $lib $u "Librarian" }

# ---- Technician ----
$tech = Login 'tech@toolshare.local' 'Tech@12345'
$techRoutes = @('/Maintenance', '/Maintenance/Tools', '/Maintenance/Records', '/Maintenance/RecordForm',
    '/Maintenance/Repairs', '/Maintenance/RepairForm', '/Maintenance/Disposals', '/Maintenance/Profile')
foreach ($u in $techRoutes) { Hit $tech $u "Technician" }

# ---- Admin ----
$adm = Login 'admin@toolshare.local' 'Lib@12345'
$admRoutes = @('/Admin', '/Admin/Users', '/Admin/Roles', '/Admin/AddUser', '/Admin/Settings',
    '/Admin/AuditLogs', '/Admin/Revenue', '/Admin/Activity', '/Admin/Promotions', '/Admin/PromotionForm',
    '/Admin/Bookings', '/Admin/Transactions', '/Admin/Tools')
foreach ($u in $admRoutes) { Hit $adm $u "Admin" }

# ---- Role isolation checks (member shouldn't reach staff pages) ----
foreach ($u in @('/Library', '/Maintenance', '/Admin')) { Hit $member $u "Member→Staff" }

$results | Format-Table -AutoSize
$fail = $results | Where-Object { $_.Status -ne 200 -and $_.Status -ne 302 }
"`n=== SUMMARY ==="
"Total: $($results.Count) | OK(200): $(($results | ? Status -eq 200).Count) | Redirect(302): $(($results | ? Status -eq 302).Count) | FAIL: $($fail.Count)"
if ($fail) { $fail | Format-Table -AutoSize }
