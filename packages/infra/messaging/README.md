# RabbitMQ Messaging Infrastructure

This directory contains the shared RabbitMQ messaging infrastructure for HappyHeadlines microservices.

## Overview

RabbitMQ serves as the message broker for asynchronous communication between services:
- **PublisherService** publishes `PublishArticleCommand` messages
- **ArticleService** consumes commands to create articles
- **NewsletterService** consumes `ArticlePublishedEvent` for notifications

## Quick Start

```bash
# Start RabbitMQ
docker-compose up -d

# View logs
docker-compose logs -f

# Stop RabbitMQ
docker-compose down
```

## Access

- **Management UI**: http://localhost:15672
  - Username: `admin` (configured in `.env`)
  - Password: `admin` (configured in `.env`)
- **AMQP Port**: 5672

## Message Flow

```
webapp-service → PublisherService → RabbitMQ (article.publish.command)
                                       ↓
                                  ArticleService (creates article)
                                       ↓
                                  RabbitMQ (article.published.event)
                                       ↓
                                  NewsletterService (sends notifications)
```

## Exchanges and Queues

### article-events Exchange
- **Type**: Topic
- **Routing Keys**:
  - `article.publish.command` → `article.publish` queue (ArticleService)
  - `article.published.event` → `newsletter.article.published` queue (NewsletterService)

## Network

Services using RabbitMQ should connect to the `happyheadlines-messaging` network.

## Configuration

Edit `.env` to change RabbitMQ credentials:
```
RABBITMQ_USER=admin
RABBITMQ_PASSWORD=admin
```
