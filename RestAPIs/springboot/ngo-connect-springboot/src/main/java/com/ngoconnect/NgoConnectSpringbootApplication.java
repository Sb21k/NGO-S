package com.ngoconnect;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.cloud.client.discovery.EnableDiscoveryClient;


@SpringBootApplication
@EnableDiscoveryClient

public class NgoConnectSpringbootApplication {

    public static void main(String[] args) {
        SpringApplication.run(NgoConnectSpringbootApplication.class, args);
    }
}

