import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { PackageResolver } from './core/resolvers/package-resolver.service';

import { HomeComponent } from './home/home.component';
import { DetailsComponent } from './details/details.component';
import { ServiceDescriptionComponent } from './service-description/service-description.component';

const routes: Routes = [
    {
        path: '',
        component: HomeComponent,
        pathMatch: 'full'
    },
    {
        path: 'service-description',
        component: ServiceDescriptionComponent,
        pathMatch: 'full'
    },
    {
        path: 'details/:id',
        component: DetailsComponent,
        resolve: {
            resolvedPackage: PackageResolver,
        },
        data: {
            packageResolverOptions: {
                redirectOnError: { commands: [''] }
            }
        },
        pathMatch: 'full'
    },
    { path: '**', redirectTo: '' }
];

@NgModule({
    imports: [
        RouterModule.forRoot(routes, {
            scrollPositionRestoration: 'enabled',
            anchorScrolling: 'enabled',
            paramsInheritanceStrategy: "always"
        })
    ],
    exports: [
        RouterModule
    ]
})
export class AppRouterModule { }
