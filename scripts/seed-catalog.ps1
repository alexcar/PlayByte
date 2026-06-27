<#
.SYNOPSIS
  Popula o catálogo da API PlayByte (bandas, álbuns e faixas) para homologação.

.DESCRIPTION
  Registra (ou reaproveita) um usuário, autentica e cria um catálogo de exemplo via
  os endpoints autenticados de /api/bands. Necessário porque o catálogo nasce vazio
  e as US de catálogo/busca/reprodução precisam de dados.

.EXAMPLE
  ./scripts/seed-catalog.ps1
  ./scripts/seed-catalog.ps1 -ApiBaseUrl "http://localhost:5080/api"
#>
param(
  [string]$ApiBaseUrl = "http://localhost:5080/api",
  [string]$Email = "seed@playbyte.com",
  [string]$Password = "Playbyte123",
  [string]$Name = "Seed Admin"
)

$ErrorActionPreference = "Stop"

function ToSeconds([string]$mmss) {
  $parts = $mmss.Split(":")
  return [int]$parts[0] * 60 + [int]$parts[1]
}

Write-Host "==> Registrando usuário de seed (ignora se já existir)..."
try {
  Invoke-RestMethod -Method Post -Uri "$ApiBaseUrl/users" -ContentType "application/json" `
    -Body (@{ name = $Name; email = $Email; password = $Password } | ConvertTo-Json) | Out-Null
} catch {
  if ($_.Exception.Response.StatusCode.value__ -ne 409) { throw }
  Write-Host "    Usuário já existe, seguindo."
}

Write-Host "==> Autenticando..."
$login = Invoke-RestMethod -Method Post -Uri "$ApiBaseUrl/auth/login" -ContentType "application/json" `
  -Body (@{ email = $Email; password = $Password } | ConvertTo-Json)
$headers = @{ Authorization = "Bearer $($login.accessToken)" }

# Catálogo de exemplo. Cada faixa: título + duração "m:ss".
$catalog = @(
  @{ name = "Queen"; albums = @(
      @{ title = "A Night at the Opera"; year = 1975; tracks = @(
          @{ t = "Death on Two Legs"; d = "3:43" },
          @{ t = "Bohemian Rhapsody"; d = "5:55" },
          @{ t = "You're My Best Friend"; d = "2:52" }) },
      @{ title = "News of the World"; year = 1977; tracks = @(
          @{ t = "We Will Rock You"; d = "2:02" },
          @{ t = "We Are the Champions"; d = "2:59" }) }) },
  @{ name = "Pink Floyd"; albums = @(
      @{ title = "The Dark Side of the Moon"; year = 1973; tracks = @(
          @{ t = "Time"; d = "6:53" },
          @{ t = "Money"; d = "6:22" },
          @{ t = "Us and Them"; d = "7:49" }) },
      @{ title = "Wish You Were Here"; year = 1975; tracks = @(
          @{ t = "Wish You Were Here"; d = "5:34" }) }) },
  @{ name = "Coldplay"; albums = @(
      @{ title = "A Rush of Blood to the Head"; year = 2002; tracks = @(
          @{ t = "The Scientist"; d = "5:09" },
          @{ t = "Clocks"; d = "5:07" }) }) },
  @{ name = "Foo Fighters"; albums = @(
      @{ title = "The Colour and the Shape"; year = 1997; tracks = @(
          @{ t = "Everlong"; d = "4:10" },
          @{ t = "Monkey Wrench"; d = "3:51" }) }) },
  @{ name = "Nirvana"; albums = @(
      @{ title = "Nevermind"; year = 1991; tracks = @(
          @{ t = "Smells Like Teen Spirit"; d = "5:01" },
          @{ t = "Come as You Are"; d = "3:39" }) }) },
  @{ name = "Metallica"; albums = @(
      @{ title = "Metallica (Black Album)"; year = 1991; tracks = @(
          @{ t = "Enter Sandman"; d = "5:31" },
          @{ t = "Nothing Else Matters"; d = "6:28" }) }) }
)

foreach ($band in $catalog) {
  Write-Host "==> Banda: $($band.name)"
  $bandResp = Invoke-RestMethod -Method Post -Uri "$ApiBaseUrl/bands" -Headers $headers -ContentType "application/json" `
    -Body (@{ name = $band.name; coverImageUrl = $null } | ConvertTo-Json)
  $bandId = $bandResp.id

  foreach ($album in $band.albums) {
    $albumResp = Invoke-RestMethod -Method Post -Uri "$ApiBaseUrl/bands/$bandId/albums" -Headers $headers -ContentType "application/json" `
      -Body (@{ title = $album.title; releaseYear = $album.year } | ConvertTo-Json)
    $albumId = $albumResp.id

    foreach ($track in $album.tracks) {
      Invoke-RestMethod -Method Post -Uri "$ApiBaseUrl/bands/$bandId/albums/$albumId/tracks" -Headers $headers -ContentType "application/json" `
        -Body (@{ title = $track.t; durationSeconds = (ToSeconds $track.d) } | ConvertTo-Json) | Out-Null
    }
    Write-Host "    Álbum: $($album.title) ($($album.tracks.Count) faixas)"
  }
}

Write-Host "`nCatálogo populado com sucesso." -ForegroundColor Green
