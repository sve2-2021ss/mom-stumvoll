use anyhow::Result;
use async_amqp::LapinAsyncStdExt;
use lapin::{Channel, Connection, ConnectionProperties, Consumer, Queue};
use lapin::options::{BasicConsumeOptions, QueueBindOptions, QueueDeclareOptions};
use lapin::types::FieldTable;

use crate::config::config::Config;

pub async fn connect(host: &str) -> lapin::Result<Connection> {
    Connection::connect(host, ConnectionProperties::default()
        .with_async_std()).await
}

async fn init_rabbitmq(config: &Config, connection: &Connection) -> Result<(Channel, Queue)> {
    let channel = connection.create_channel().await?;
    let queue = channel.queue_declare(
        "",
        queue_declare_options(),
        FieldTable::default(),
    ).await?;

    channel.queue_bind(
        queue.name().as_str(),
        &config.exchange_name,
        &config.routing_key,
        QueueBindOptions::default(),
        FieldTable::default(),
    ).await?;

    Ok((channel, queue))
}

pub async fn create_consumer(config: &Config, connection: &Connection) -> Result<Consumer> {
    let (channel, queue) = init_rabbitmq(&config, &connection).await?;
    let consumer = channel.basic_consume(
        queue.name().as_str(),
        "watchdog",
        BasicConsumeOptions::default(),
        FieldTable::default(),
    ).await?;

    Ok(consumer)
}

fn queue_declare_options() -> QueueDeclareOptions {
    QueueDeclareOptions {
        exclusive: true,
        auto_delete: true,
        durable: false,
        nowait: false,
        passive: false,
    }
}

