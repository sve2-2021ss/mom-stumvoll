package watchdog

import Config
import api.*
import kotlinx.serialization.ExperimentalSerializationApi
import watchdog.notifications.NotificationStrategy

@ExperimentalSerializationApi
class Watchdog(private val config: Config, private val notificationStrategy: NotificationStrategy) {
    fun onNewValue(deviceName: String, systemValue: SystemValue) {
        val errorList = when (systemValue) {
            is Cpu -> checkCpu(systemValue)
            is Ram -> checkRam(systemValue)
            is ServiceEvent -> checkServiceEvent(systemValue)
        }

        if (errorList.isNotEmpty()) {
            notificationStrategy.notify(deviceName, errorList)
        }
    }

    private fun checkCpu(cpu: Cpu): List<String> {
        val errorList = mutableListOf<String>()
        cpu.coreTemps.average().let {
            if (it > config.cpuConfig.maxTmp) {
                errorList.add("Cpu temperature $it above threshold ${config.cpuConfig.maxTmp}")
            }
        }

        if (cpu.loadPercentage > config.cpuConfig.maxLoad) {
            errorList.add("Cpu load ${cpu.loadPercentage} above threshold ${config.cpuConfig.maxLoad}")
        }

        if (cpu.powerDraw > config.cpuConfig.maxPower) {
            errorList.add("Cpu power draw ${cpu.powerDraw} above threshold ${config.cpuConfig.maxPower}")
        }

        return errorList
    }

    private fun checkRam(ram: Ram): List<String> {
        val errorList = mutableListOf<String>()
        (100 * ram.usedMb / ram.totalMb).let {
            if (it > config.ramConfig.maxLoad) {
                errorList.add("Ram load $it above threshold ${config.ramConfig.maxLoad}")
            }
        }

        if (ram.memoryClock > config.ramConfig.maxClock) {
            errorList.add("Ram clock ${ram.memoryClock} above threshold ${config.ramConfig.maxClock}")
        }

        return errorList
    }

    private fun checkServiceEvent(serviceEvent: ServiceEvent): List<String> =
        if (serviceEvent.serviceEventType == ServiceEventType.Stop
            && config.delicateProcesses.contains(serviceEvent.executable)
        ) {
            listOf("Critical service ${serviceEvent.executable} was stopped")
        } else listOf()

}