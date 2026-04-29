# Безопасность

Если вы нашли уязвимость в учебном проекте, опишите её через приватное сообщение преподавателю или через issue без раскрытия эксплойта в заголовке.

В CI **нет** захардкоженных паролей и ключей; для Actions используется стандартный **`GITHUB_TOKEN`** с ограниченными правами (`permissions` в [`.github/workflows/ci.yml`](.github/workflows/ci.yml)). Секреты репозитория (**Settings → Secrets**) в этом workflow не используются — деплой в облако из CI не настроен; при появлении выдачи кредов в прод лучше смотреть на **OIDC** к облаку вместо долгоживущих ключей в секретах (в job для OIDC добавляют **`permissions: id-token: write`** — см. [OIDC в GitHub Actions](https://docs.github.com/en/actions/deployment/security-hardening-your-deployments/about-security-hardening-with-openid-connect)).

Не выкладывайте в логи значения секретов и не печатайте их в шагах workflow (`echo ${{ secrets.* }}` — плохая идея даже для отладки).
