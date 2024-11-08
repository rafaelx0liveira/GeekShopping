# Projeto: Arquitetura de Microsserviços com .NET 8 e ASP.NET Core

Este repositório implementa uma arquitetura de microsserviços utilizando **.NET 8**, **ASP.NET Core**, **Docker**, **Ocelot** como API Gateway, **IdentityServer4** e várias outras ferramentas de integração. O projeto segue as melhores práticas para construção de sistemas escaláveis e descentralizados, ideal para aplicações modernas de alta disponibilidade.

## Sumário

- [Projeto: Arquitetura de Microsserviços com .NET 8 e ASP.NET Core](#projeto-arquitetura-de-microsserviços-com-net-8-e-aspnet-core)
  - [Sumário](#sumário)
  - [Visão Geral](#visão-geral)
  - [Arquitetura](#arquitetura)
  - [Diagrama de classe](#diagrama-de-classe)
    - [Diagrama de classe da camada Web](#diagrama-de-classe-da-camada-web)
      - [Controladores](#controladores)
      - [Modelos](#modelos)
      - [Serviços](#serviços)
      - [Utilitários](#utilitários)
    - [Diagrama de classe da camada de Message Bus](#diagrama-de-classe-da-camada-de-message-bus)
    - [Diagrama de classe da camada de Processador de Pagamentos](#diagrama-de-classe-da-camada-de-processador-de-pagamentos)
    - [Diagrama de classe da API de Carrinho de Compras](#diagrama-de-classe-da-api-de-carrinho-de-compras)
    - [Diagrama de classe da API de Cupons](#diagrama-de-classe-da-api-de-cupons)
    - [Diagrama de classe do Serviço de E-mail](#diagrama-de-classe-do-serviço-de-e-mail)
    - [Diagrama de classe IdentityServer](#diagrama-de-classe-identityserver)
    - [Diagrama de classe da API de Pedidos](#diagrama-de-classe-da-api-de-pedidos)
    - [Diagrama de classe da API de Pagamentos](#diagrama-de-classe-da-api-de-pagamentos)
    - [Diagrama de classe da API de Produtos](#diagrama-de-classe-da-api-de-produtos)
  - [Tecnologias Utilizadas](#tecnologias-utilizadas)
  - [Microsserviços](#microsserviços)
    - [Gateway e Infraestrutura](#gateway-e-infraestrutura)
    - [Serviços](#serviços-1)
  - [Fluxo de Dados e Comunicação](#fluxo-de-dados-e-comunicação)
    - [Tipos de Exchanges](#tipos-de-exchanges)
  - [Autenticação e Autorização](#autenticação-e-autorização)
    - [Provedor de Identidade](#provedor-de-identidade)
  - [Observabilidade e Monitoramento](#observabilidade-e-monitoramento)
  - [Como Executar o Projeto](#como-executar-o-projeto)
    - [Pré-requisitos](#pré-requisitos)
    - [Passo a Passo](#passo-a-passo)
    - [Testando as APIs](#testando-as-apis)
  - [Contribuição](#contribuição)

## Visão Geral

Este projeto é uma implementação de uma arquitetura de microsserviços com foco em **escalabilidade** e **baixa acoplamento entre os serviços**. Ele fornece uma base robusta para construir e gerenciar microsserviços de forma fácil, com comunicação segura, autenticação centralizada e monitoramento integrado.

## Arquitetura

![Diagrama da Arquitetura](./Docs/diagrama-de-arquitetura.jpg)

A arquitetura é baseada em **microsserviços desacoplados** que se comunicam através de um **API Gateway** e um **bus de mensagens**.

**Principais componentes**:
- **API Gateway (Ocelot)**: Ponto de entrada para os clientes, roteando as solicitações para os serviços adequados.
- **Microsserviços**: Implementados em .NET 8, cada um responsável por uma parte específica da aplicação.
- **Bus de Mensagens (RabbitMQ)**: Integração e comunicação entre microsserviços.
- **Autenticação**: Centralizada com **IdentityServer4**.
- **Monitoramento e Observabilidade**: Stack de monitoramento para garantir confiabilidade em produção.

## Diagrama de classe

O diagrama de classe representa a estrutura dos microsserviços e a relação entre eles:

### Diagrama de classe da camada Web

Obs.: Possível dar zoom na imagem para melhor visualização.

![alt text](./Docs/Diagrama-de-classe-web/diagrama-de-classe-web.png)

#### Controladores

![alt text](./Docs/Diagrama-de-classe-web/Controller/CartController.png)

![alt text](./Docs/Diagrama-de-classe-web/Controller/HomeController.png)

![alt text](./Docs/Diagrama-de-classe-web/Controller/ProductController.png)

#### Modelos

![alt text](./Docs/Diagrama-de-classe-web/Models/CartDetailViewModel.png)

![alt text](./Docs/Diagrama-de-classe-web/Models/CartHeaderViewModel.png)

![alt text](./Docs/Diagrama-de-classe-web/Models/CartViewModel.png)

![alt text](./Docs/Diagrama-de-classe-web/Models/CouponViewModel.png)

![alt text](./Docs/Diagrama-de-classe-web/Models/ErrorViewModel.png)

![alt text](./Docs/Diagrama-de-classe-web/Models/ProductViewModel.png)

#### Serviços

![alt text](./Docs/Diagrama-de-classe-web/Services/CartService.png)

![alt text](./Docs/Diagrama-de-classe-web/Services/CouponService.png)

![alt text](./Docs/Diagrama-de-classe-web/Services/ProductService.png)

#### Utilitários

![alt text](./Docs/Diagrama-de-classe-web/Utils/HttpClientExtensions.png)

![alt text](./Docs/Diagrama-de-classe-web/Utils/Role.png)

### Diagrama de classe da camada de Message Bus

![alt text](./Docs/diagrama-de-classe-message-bus/diagrama-de-classe-message-bus.png)

### Diagrama de classe da camada de Processador de Pagamentos

![alt text](./Docs/diagrama-de-classe-processor-payments/diagrama-de-classe-processor-payments.png)

### Diagrama de classe da API de Carrinho de Compras

![alt text](./Docs/diagrama-de-classe-cart-api/diagrama-de-classe-cart-api.png)

![alt text](./Docs/diagrama-de-classe-cart-api/CartController.png)

![alt text](./Docs/diagrama-de-classe-cart-api/rabbitMQMessageSender.png)

![alt text](./Docs/diagrama-de-classe-cart-api/CartRepository.png)

![alt text](./Docs/diagrama-de-classe-cart-api/CouponRepository.png)

### Diagrama de classe da API de Cupons

![alt text](./Docs/diagrama-de-classe-coupon-api/diagrama-de-classe-coupon-api.png)

![alt text](./Docs/diagrama-de-classe-coupon-api/CouponController.png)

![alt text](./Docs/diagrama-de-classe-coupon-api/CouponRepository.png)

### Diagrama de classe do Serviço de E-mail

![alt text](./Docs/diagrama-de-classe-email/diagrama-de-classe-email.png)

![alt text](./Docs/diagrama-de-classe-email/rabbitMQMessagePaymentConsumer.png)

![alt text](./Docs/diagrama-de-classe-email/EmailRepository.png)

### Diagrama de classe IdentityServer

![alt text](./Docs/diagrama-de-classe-identity-server/diagrama-de-classe-identity-server.png)

### Diagrama de classe da API de Pedidos

![alt text](./Docs/diagrama-de-classe-order-api/diagrama-de-classe-order-api.png)

![alt text](./Docs/diagrama-de-classe-order-api/rabbitMQCheckoutConsumer.png)

![alt text](./Docs/diagrama-de-classe-order-api/rabbitMQPaymentConsumer.png)

![alt text](./Docs/diagrama-de-classe-order-api/rabbitMQMessageSender.png)

![alt text](./Docs/diagrama-de-classe-order-api/OrderRepository.png)

### Diagrama de classe da API de Pagamentos

![alt text](./Docs/diagrama-de-classe-payment-api/diagrama-de-classe-payment-api.png)

![alt text](./Docs/diagrama-de-classe-payment-api/rabbitMQPaymentConsumer.png)

![alt text](./Docs/diagrama-de-classe-payment-api/rabbitMQMessageSender.png)

### Diagrama de classe da API de Produtos

![alt text](./Docs/diagrama-de-classe-product-api/diagrama-de-classe-product-api.png)

![alt text](./Docs/diagrama-de-classe-product-api/ProductController.png)

![alt text](./Docs/diagrama-de-classe-product-api/ProductRepository.png)


## Tecnologias Utilizadas

- **ASP.NET Core** (para desenvolvimento de APIs REST)
- **.NET 8** (base para desenvolvimento de serviços)
- **Docker** (para containerização do serviço do RabbitMQ)
- **RabbitMQ** (mensageria entre os microsserviços)
- **MySQL** (persistência de dados, gerenciado com MySQL Workbench)
- **IdentityServer4** (provedor de autenticação e autorização centralizado)
- **Ocelot** (API Gateway)
- **Swagger** (documentação das APIs)

## Microsserviços

### Gateway e Infraestrutura

1. **APIGateway**: Implementado com **Ocelot** para centralizar as requisições e roteamento para os microsserviços.
2. **MessageBus**: Serviço de bus de mensagens para comunicação entre microsserviços.
3. **PaymentsProcessor**: Processador de pagamentos responsável por validar e processar transações de pagamento.

### Serviços

1. **CartAPI**: API de gerenciamento de carrinho de compras.
2. **CouponAPI**: API para gerenciamento e validação de cupons de desconto.
3. **Email**: Serviço de envio de e-mails para notificações transacionais.
4. **IdentityServer**: Serviço de autenticação e autorização, utilizando **IdentityServer4**.
5. **OrderAPI**: API de gerenciamento de pedidos.
6. **PaymentAPI**: API de processamento de pagamentos.
7. **ProductAPI**: API de gerenciamento de produtos.

Cada microsserviço possui sua **própria base de dados MySQL** e é independente, garantindo o isolamento de falhas e possibilitando escalabilidade individual.

## Fluxo de Dados e Comunicação

A comunicação entre os serviços ocorre de duas maneiras:

- **APIGateway com Ocelot**: Centraliza o roteamento de requisições HTTP, garantindo que clientes e usuários interajam com uma única API.
- **RabbitMQ**: Facilita a comunicação assíncrona e baseada em eventos entre microsserviços, permitindo desacoplamento e resiliência.

Claro! Vou incluir informações sobre os tipos de exchanges que você desenvolveu, como o **fanout** e o **direct**, na documentação do seu projeto. Aqui está um exemplo de como você pode documentar isso:


### Tipos de Exchanges

No projeto, foram implementados dois tipos de exchanges no RabbitMQ, que são fundamentais para o roteamento de mensagens:

1. **Fanout Exchange**:
    - **Descrição**: O fanout exchange roteia mensagens para todas as filas que estão ligadas a ele, sem considerar as regras de roteamento. Esse tipo de exchange é ideal para sistemas de broadcast, onde a mesma mensagem deve ser enviada a vários consumidores.
    - **Uso**: Quando uma mensagem é publicada em uma fanout exchange, todas as filas associadas recebem uma cópia da mensagem, permitindo que múltiplos consumidores processam a mesma informação simultaneamente.

2. **Direct Exchange**:
    - **Descrição**: O direct exchange roteia mensagens para filas específicas com base em uma chave de roteamento exata. As mensagens são enviadas a filas que possuem uma ligação com a exchange e correspondem à chave de roteamento fornecida.
    - **Uso**: Esse tipo de exchange é útil quando é necessário direcionar mensagens a filas específicas, permitindo um controle mais refinado sobre como as mensagens são distribuídas entre os consumidores.

## Autenticação e Autorização

A aplicação utiliza **IdentityServer4** para autenticação centralizada, fornecendo **tokens JWT** para garantir acesso seguro entre microsserviços.

### Provedor de Identidade

1. **Usuários e Funções**: Gerenciamento de usuários e funções para controle de acesso.
2. **Integração com Microsserviços**: Cada microsserviço valida o token JWT para assegurar autenticação e autorização.

## Observabilidade e Monitoramento

Para monitorar e rastrear o sistema em produção, foram implementadas soluções de monitoramento e logging, com possibilidade de integração com ferramentas como Prometheus e Grafana para coleta e visualização de métricas.

## Como Executar o Projeto

### Pré-requisitos

- **Docker e Docker Compose**
- **MySQL Workbench**
- **Visual Studio 2022** ou outra IDE compatível com .NET 8
- **RabbitMQ**

### Passo a Passo

1. Clone o repositório:

    ```bash
        git clone https://github.com/rafaelx0liveira/GeekShopping.git
        cd seu-repositorio
    ```

2. Execute os containers usando Docker Compose:

    ```bash
        docker-compose up -d
    ```

3. Acesse a aplicação no endereço [https://localhost:4430](https://localhost:4430).

### Testando as APIs

Cada microsserviço possui uma documentação com Swagger disponível:

- **APIGateway**: `https://localhost:4480`
- **CartAPI**: `https://localhost:4445/swagger`
- **CouponAPI**: `https://localhost:4450/swagger`
- **Email Service**: `https://localhost:4460/swagger`
- **IdentityServer**: `https://localhost:4435`
- **OrderAPI**: `http://localhost:4455/swagger`
- **PaymentAPI**: `http://localhost:5109/swagger`
- **ProductAPI**: `https://localhost:4440/swagger`

## Contribuição

Contribuições são bem-vindas! Para contribuir:

1. Faça um fork do projeto.
2. Crie uma branch para suas alterações: `git checkout -b minha-feature`.
3. Faça o commit das alterações: `git commit -m 'Minha nova feature'`.
4. Envie para o repositório: `git push origin minha-feature`.
5. Abra um Pull Request para revisão.
