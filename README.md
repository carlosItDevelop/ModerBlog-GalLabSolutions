
# 📝 ModernBlog

Um sistema de blog moderno e responsivo desenvolvido com ASP.NET Core 7, Entity Framework Core e PostgreSQL.

![ModernBlog](wwwroot/images/imagem1.png)

## 🚀 Funcionalidades

### ✨ Para Visitantes
- Visualização de posts publicados com paginação
- Sistema de categorias e tags
- Posts em destaque (featured)
- Posts mais recentes
- Sistema de likes (requer login)
- Interface responsiva e moderna
- Busca por categorias

### 👤 Para Usuários Autenticados
- Sistema de autenticação completo (registro/login)
- Curtir posts
- Perfil de usuário personalizado

### 🔧 Para Administradores
- Painel administrativo completo
- Gerenciamento de posts (CRUD)
- Editor de conteúdo rico
- Upload de imagens
- Gerenciamento de categorias e tags
- Sistema de publicação/rascunho
- Controle de posts em destaque

## 🛠️ Tecnologias Utilizadas

### Backend
- **ASP.NET Core 7** - Framework web
- **Entity Framework Core** - ORM
- **PostgreSQL** - Banco de dados
- **ASP.NET Core Identity** - Autenticação e autorização

### Frontend
- **Bootstrap 5.3.2** - Framework CSS
- **Bootstrap Icons** - Ícones
- **Razor Pages** - Engine de templates
- **JavaScript vanilla** - Interatividade

### Ferramentas
- **Replit** - Ambiente de desenvolvimento
- **Nix** - Gerenciamento de pacotes

## 🏗️ Arquitetura

```
ModernBlog/
├── Areas/Admin/           # Área administrativa
│   ├── Controllers/       # Controladores admin
│   └── Views/            # Views admin
├── Controllers/          # Controladores públicos
├── Data/                # Contexto do banco e seeding
├── Models/              # Modelos de dados
├── Services/            # Serviços de negócio
├── Views/               # Views públicas
└── wwwroot/            # Arquivos estáticos
```

## 📊 Modelo de Dados

### Entidades Principais

- **ApplicationUser**: Usuários do sistema (herda IdentityUser)
- **Post**: Posts do blog
- **Category**: Categorias dos posts
- **Tag**: Tags dos posts
- **Comment**: Comentários (preparado para implementação futura)
- **PostLike**: Sistema de likes
- **PostTag**: Relacionamento many-to-many entre posts e tags

### Relacionamentos

```
User 1:N Post
Category 1:N Post
Post N:M Tag (através de PostTag)
User N:M Post (através de PostLike para likes)
```

## 🔧 Configuração e Instalação

### Pré-requisitos
- .NET 7 SDK
- PostgreSQL

### Configuração do Banco de Dados

1. **Replit (Automático)**:
   - O sistema detecta automaticamente a variável `DATABASE_URL`
   - Configuração automática do PostgreSQL

2. **Local**:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=ModernBlog;Username=postgres;Password=postgres"
     }
   }
   ```

### Executando o Projeto

1. **No Replit**:
   - Clique no botão "Run"

2. **Localmente**:
   ```bash
   dotnet restore
   dotnet run
   ```

### Dados Iniciais

O sistema inclui dados de exemplo que são carregados automaticamente:
- Usuário administrador padrão
- 4 categorias (Tecnologia, Negócios, Lifestyle, Educação)
- 5 tags de exemplo
- 8 posts de exemplo

#### Credenciais do Administrador
- **Email**: admin@modernblog.com
- **Senha**: Admin123!

## 🎨 Interface

### Design System
- **Cores Primárias**: Azul (#007bff) e tons de cinza
- **Tipografia**: Sistema nativo (-apple-system, BlinkMacSystemFont, Segoe UI, Roboto)
- **Componentes**: Bootstrap 5.3.2 com customizações
- **Ícones**: Bootstrap Icons

### Responsividade
- Mobile-first design
- Breakpoints do Bootstrap
- Navegação adaptável
- Cards responsivos

## 📋 Funcionalidades Detalhadas

### Sistema de Posts
- **CRUD completo** para administradores
- **Rich text editor** para conteúdo
- **Upload de imagens** com validação
- **Sistema de slug** para URLs amigáveis
- **Publicação programada**
- **Posts em destaque**
- **Contadores de visualizações e likes**

### Sistema de Usuários
- **Registro e login** com ASP.NET Identity
- **Perfis de usuário** com bio e avatar
- **Roles**: User e Admin
- **Senha segura** com requisitos configuráveis

### Performance
- **Paginação** eficiente
- **Eager loading** otimizado
- **Índices** no banco de dados
- **Lazy loading** de imagens

## 🔐 Segurança

- Autenticação via ASP.NET Core Identity
- Autorização baseada em roles
- Validação de entrada
- Proteção CSRF
- Sanitização de HTML

## 🚀 Deploy

### Replit Deploy
1. Configure as variáveis de ambiente necessárias
2. Use o recurso de Deploy do Replit
3. O sistema é automaticamente configurado para produção

### Configurações de Produção
- HTTPS obrigatório
- Compressão de response
- Cache de arquivos estáticos
- Logs estruturados

## 📈 Futuras Implementações

- [ ] Sistema de comentários completo
- [ ] Busca avançada com filtros
- [ ] Newsletter e notificações
- [ ] SEO otimizado com meta tags
- [ ] API REST para mobile
- [ ] Sistema de moderação
- [ ] Analytics e relatórios
- [ ] Múltiplos idiomas
- [ ] Tema escuro

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📝 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

## 👨‍💻 Autor

Desenvolvido com ❤️ para a comunidade de desenvolvedores.

---

## 📞 Suporte

Se você encontrar algum problema ou tiver sugestões:
- Abra uma [issue](../../issues)
- Entre em contato via email

**Visite o blog**: [ModernBlog no Replit](https://replit.com/@seu-usuario/ModernBlog)
