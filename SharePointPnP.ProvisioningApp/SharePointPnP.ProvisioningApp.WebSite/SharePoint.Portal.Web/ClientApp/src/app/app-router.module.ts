import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { PackageResolver } from './core/resolvers/package-resolver.service';

import { HomeComponent } from './home/home.component';
import { DetailsComponent } from './details/details.component';

const routes: Routes = [
    {
        path: '',
        component: HomeComponent,
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
            anchorScrolling: 'enabled'
        })
    ],
    exports: [
        RouterModule
    ]
})
export class AppRouterModule { }
