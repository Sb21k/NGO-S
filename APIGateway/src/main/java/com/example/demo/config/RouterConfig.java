package com.example.demo.config;

import java.util.Arrays;

import org.springframework.cloud.gateway.route.RouteLocator;
import org.springframework.cloud.gateway.route.builder.RouteLocatorBuilder;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.web.cors.CorsConfiguration;
import org.springframework.web.cors.reactive.CorsWebFilter;
import org.springframework.web.cors.reactive.UrlBasedCorsConfigurationSource;

@Configuration
public class RouterConfig {
	
	@Bean
	public RouteLocator createRoutes(RouteLocatorBuilder builder) {
		
		return builder.routes()
			   .route("NGOService", r-> r.path("/ngo/**")
					   //.uri("http://localhost:8081"))
					   .uri("lb://NGOService"))
			   .route("BeneficiaryService", r->r.path("/beneficiary/**")
					   //.uri("http://localhost:8082"))
					   .uri("lb://BeneficiaryService"))
			   .route("springboot_admin_backend", r->r.path("/admin/**")
					   //.uri("http://localhost:8082"))
					   .uri("lb://springboot_admin_backend"))
			   .route("dotnet-auth",r->r.path("/Auth/**")
					   .uri("lb://DOTNET-AUTH-SERVICE"))
			   .route("DONOR-BACKEND", r->r.path("/donor/**")
					   //.uri("http://localhost:8082"))
					   .uri("lb://DONOR-BACKEND"))
			
			   .build();
		
		
		
	}
	
	@Bean
	public CorsWebFilter corsWebFilter() {
		UrlBasedCorsConfigurationSource source = new UrlBasedCorsConfigurationSource();
		CorsConfiguration config = new CorsConfiguration();
		config.setAllowedOrigins(Arrays.asList("http://localhost:5173"));
		config.setAllowedMethods(Arrays.asList("GET","POST","PUT","DELETE","OPTIONS"));
		config.setAllowedHeaders(Arrays.asList("Authorization","Content-Type"));
		config.setExposedHeaders(Arrays.asList("Authorization"));
		source.registerCorsConfiguration("/**", config);
		return new CorsWebFilter(source);
		
		
	}

}
