use std::io;

use anyhow::Result;
use async_std::task;
use async_std::task::JoinHandle;
use futures_util::StreamExt;
use lapin::Consumer;
use lapin::message::Delivery;
use lapin::options::BasicAckOptions;
use regex::Regex;

use crate::api::lib::{connect, create_consumer};
use crate::api::parser::parse_data;
use crate::config::config::Config;
use crate::watchdog::watchdog::Watchdog;

mod config;
mod watchdog;
mod system_values;
mod api;

#[async_std::main]
async fn main() -> Result<()> {
    let config_string = std::fs::read_to_string("config.json")?;
    let config: Config = serde_json::from_str(&*config_string)?;

    let connection = connect(config.host.as_str()).await?;
    let consumer = create_consumer(&config, &connection).await?;


    let task = start_consumer(consumer, Box::new(config));

    block();
    task.cancel().await;
    connection.close(0, "bye,bye").await?;
    Ok(())
}

pub fn start_consumer(mut consumer: Consumer, config: Box<Config>) -> JoinHandle<Result<()>> {
    let watchdog = Watchdog::new(config);

    task::spawn(async move {
        while let Some(Ok(message)) = consumer.next().await {
            handle_message(message.1, &watchdog).await?
        }

        Ok(())
    })
}

async fn handle_message(delivery: Delivery, watchdog: &Watchdog) -> Result<()> {
    delivery.ack(BasicAckOptions::default()).await?;

    if let Some(name) = get_device_name(delivery.routing_key.as_str()) {
        let value = parse_data(delivery.routing_key.as_str(), delivery.data)?;
        watchdog.on_system_value(&name, &value);
    }

    Ok(())
}

fn get_device_name(key: &str) -> Option<String> {
    let regex = Regex::new("(.*?)\\..*").unwrap();
    let captures = regex.captures(key)?;
    Some(captures.get(1)?.as_str().to_string())
}

fn block() {
    io::stdin().read_line(&mut String::new()).unwrap();
}