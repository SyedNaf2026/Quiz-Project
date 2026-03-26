using BCrypt.Net;

var passwords = new[]
{
    ("dayanand123"),
    ("kamalesh123"),
    ("padmakumar123"),
    ("aravind123"),
    ("harini123"),
    ("syedarshed123")
};

foreach (var p in passwords)
    Console.WriteLine($"{p}|{BCrypt.HashPassword(p)}");
