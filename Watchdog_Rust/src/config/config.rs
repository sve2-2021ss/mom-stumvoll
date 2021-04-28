use serde::Deserialize;

#[derive(Deserialize)]
pub struct Config {
    pub host: String,
    pub exchange_name: String,
    pub routing_key: String,
    pub cpu_max: i32,
    pub ram_max: i32,
    pub delicate_processes: Vec<String>,
}