# PlayByte — Frontend (Angular 19 + PrimeNG 19)

Template de interface da plataforma de streaming de música **PlayByte**, construído com **Angular 19** (standalone components) e **PrimeNG 19** (styled mode com design tokens).

Este projeto replica, em Angular + PrimeNG, o template originalmente prototipado em HTML5 + Bootstrap 5.3, mantendo a mesma identidade visual (tema escuro, paleta verde streaming `#1DB954` + azul `#2E75B6`, tipografia Syne + Inter).

## Requisitos

- Node.js 18.19+ ou 20.11+ (recomendado: 20 LTS)
- npm 10+

## Como executar

```bash
# 1. Instalar dependências
npm install

# 2. Subir o servidor de desenvolvimento
npm start
# ou: npx ng serve

# 3. Acessar no navegador
# http://localhost:4200
```

A aplicação abre na landing page (`/inicio`). A navegação entre todas as telas está funcional.

## Build de produção

```bash
npm run build
# Saída em: dist/playbyte
```

## Rotas disponíveis

| Rota | Tela |
|------|------|
| `/inicio` | Landing page (hero + features + bandas em destaque) |
| `/login` | Login |
| `/cadastro` | Criação de conta |
| `/esqueci-senha` | Recuperação de senha (US-03) |
| `/redefinir-senha` | Redefinição de senha — use `?expired=1` para ver o estado de link expirado |
| `/planos` | Planos (cards + comparativo + FAQ) |
| `/catalogo` | Catálogo de bandas (com drawer de álbuns/faixas) |
| `/player` | Player de reprodução (now playing + fila) |
| `/playlists` | Playlists (lista + detalhe + criação) |
| `/favoritos` | Favoritos (abas Músicas / Bandas) |
| `/busca` | Busca de bandas e músicas |
| `/perfil` | Perfil do usuário (assinatura, segurança, notificações, histórico) |

## Estrutura do projeto

```
src/app/
├── core/
│   ├── models/          # Interfaces (User, Band, Album, Track, Playlist, Plan...)
│   └── services/        # Serviços mock com signals (auth, catalog, player...)
├── layout/
│   ├── main-layout/     # Navbar + container das telas autenticadas/públicas
│   └── auth-layout/     # Layout enxuto para telas de autenticação
├── shared/components/   # Componentes reutilizáveis (track-list, band-card...)
├── features/            # Uma pasta por tela
│   ├── auth/            # login, register, forgot-password, reset-password
│   ├── home/  plans/  catalog/  player/
│   ├── playlists/  favorites/  search/  profile/
├── playbyte-theme.ts    # Preset PrimeNG customizado (paleta PlayByte)
├── app.config.ts        # Providers (router, http, animations, PrimeNG)
└── app.routes.ts        # Roteamento com lazy loading
```

## Decisões técnicas

- **Standalone components**: sem NgModules, seguindo o padrão atual do Angular.
- **Signals**: estado reativo dos serviços (`signal`, `computed`, `toSignal`) em vez de `BehaviorSubject`.
- **Lazy loading**: cada tela é carregada sob demanda via `loadComponent`.
- **Tema PrimeNG (styled mode)**: o `playbyte-theme.ts` deriva do preset **Aura** e sobrescreve a paleta primária e as superfícies escuras. O modo escuro é ativado pela classe `.pb-dark` no `<body>`.
- **Serviços mock**: os dados vêm de JSON em `src/assets/data` e de serviços em memória, simulando latência. Não há backend — o objetivo é o template navegável.

## Regras de negócio refletidas no template

- **Plano gratuito vs. pago**: usuários do plano gratuito navegam pelo catálogo, criam playlists, favoritam e buscam, mas a **reprodução de músicas é bloqueada** com convite ao upgrade (banner no catálogo, aviso no player, toast ao tentar reproduzir).
- **Recuperação de senha** (US-03): fluxo completo de solicitação → confirmação → redefinição, incluindo o estado de **link expirado**.
- **Busca** (US-16): case-insensitive, com medição de tempo de resposta exibida ao usuário.

## Observações

- O pacote `@primeng/themes@19` aparece como *deprecated* no npm (a recomendação futura é `@primeuix/themes`), mas funciona normalmente com PrimeNG 19. Para um projeto novo de longo prazo, considere a migração quando atualizar o PrimeNG.
- Para integrar com a API .NET, basta substituir os métodos dos serviços em `core/services` por chamadas `HttpClient` reais — as assinaturas (`Observable<T>`) já foram desenhadas com isso em mente.
