# Cafeteria API

WebAPI REST desenvolvida em .NET 10 para gerenciamento de cardápio e pedidos de uma cafeteria, substituindo anotações em
papel por um fluxo estruturado do balcão até a entrega.

## Arquitetura e Organização

O projeto segue uma organização por **Features** (Vertical Slices / separação por domínio de funcionalidade):

- **Products (Menu):** cadastro, consulta, atualização e disponibilidade dos itens do cardápio.
- **Orders:** criação, consulta, transição de estados e cancelamento de pedidos, além de cálculo do total e
  enriquecimento de endereço via CEP.
- **Users:** gerenciamento de usuários e perfis de acesso (`Customer`, `Attendant`, `Barista`, `Manager`).

## Regras de Negócio e Funcionalidades Implementadas

- [x] Não é permitido adicionar produtos indisponíveis ao pedido.
- [x] Todo pedido deve conter pelo menos 1 item com quantidade maior que zero.
- [x] O valor total do pedido é calculado no backend pela API.
- [x] Alterações no preço de um produto não afetam pedidos já registrados (o preço unitário é snapshot/registrado no
  item do pedido).
- [x] Ciclo de vida do pedido modelado por `enum`: `Received` → `Preparing` → `Ready` → `Delivered`, além de `Canceled`.
- [x] Cancelamento permitido apenas quando o pedido está no estado `Received`.
- [x] Perfis/papéis de usuário contemplados: `Customer`, `Attendant`, `Barista` e `Manager`.
- [x] Integração com o **ViaCEP** para consulta/preenchimento de endereço a partir do CEP em pedidos de entrega.
- [x] Paginação, ordenação e filtros nas consultas de `Orders` e `Products`.
- [x] Documentação interativa da API via **Swagger / OpenAPI**.
- [x] Ambiente containerizado com **Docker Compose** (PostgreSQL 16 + API).
- [x] *Seeder* inicial com usuário `Manager` e 50 produtos cadastrados no cardápio.

## Como Rodar a Aplicação

### Pré-requisitos

- [Docker](https://www.docker.com/) e Docker Compose instalados.

### Executando com Docker Compose

Clone o repositório
```
git clone https://github.com/ianify/cafeteria
```
Acesse o diretorio
```
cd <PASTA_DO_PROJETO>
```
Execute o comando do docker compose
```
docker compose up -d --build
```# cafeteria
